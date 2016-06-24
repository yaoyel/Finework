using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Security;
using System.Data.Entity;
using AppBoot.Common;
using AVOSCloud.RealtimeMessageV2;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class TaskAlarmManager : AefEntityManager<TaskAlarmEntity, Guid>, ITaskAlarmManager
    {
        public TaskAlarmManager(ISessionProvider<AefSession> dbContextProvider,
            ITaskManager taskManager,
            IStaffManager staffManager,
            IPartakerManager partakerManager,
            ITaskLogManager taskLogManager,
            IIMService imService,
            INotificationManager notificatioinManager,
            IConfiguration config )
            : base(dbContextProvider)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (taskLogManager == null) throw new ArgumentNullException(nameof(taskLogManager));
            if (imService == null) throw new ArgumentException(nameof(imService));
            if (config == null) throw new ArgumentException(nameof(config));

            this.TaskManager = taskManager;
            this.StaffManager = staffManager;
            this.PartakerManager = partakerManager;
            this.TaskLogManager = taskLogManager;
            this.IMService = imService;
            this.NotificationManager = notificatioinManager;
            this.Config = config;
        }

        private ITaskManager TaskManager { get; }

        private IStaffManager StaffManager { get; }

        private IPartakerManager PartakerManager { get; }

        private  ITaskLogManager TaskLogManager { get; }

        private IIMService IMService { get; }

        private  INotificationManager NotificationManager;

        private IConfiguration Config;

        public TaskAlarmEntity CreateTaskAlarm(CreateTaskAlarmModel taskAlarmModel)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskAlarmModel.TaskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.StaffManager, taskAlarmModel.StaffId).ThrowIfFailed().Staff;
            var partaker=PartakerExistsResult.CheckForStaff(task, taskAlarmModel.StaffId).ThrowIfFailed().Partaker;
            string conversationId = task.ConversationId;

            taskAlarmModel.Receivers?.ToList().ForEach(p =>
            {
                var kind = (PartakerKinds) p;
                if (task.Partakers.All(t => t.Kind != kind))
                    throw new FineWorkException($"任务不存在{kind.GetLabel()}");
            });

            var taskAlarm = new TaskAlarmEntity();
            taskAlarm.Id = Guid.NewGuid();
            taskAlarm.Task = task;
            taskAlarm.Staff = staff;
            taskAlarm.TaskAlarmKind = taskAlarmModel.AlarmKind;
            taskAlarm.Content = taskAlarmModel.Content;

            if(taskAlarmModel.Receivers!=null)
            taskAlarm.ReceiversArray = taskAlarmModel.Receivers;

            //自定义预警
            if (taskAlarmModel.AlarmKind == TaskAlarmKinds.Custom)
            {
                taskAlarm.AlarmDesc = taskAlarmModel.AlarmDesc;
                taskAlarm.TaskAlarmKind = TaskAlarmKinds.Custom;
            }


            //非绿灯的情况下创建聊天室
            if (taskAlarmModel.AlarmKind != TaskAlarmKinds.GreenLight)
            {
                if (GetRoomKindByReceivers(taskAlarmModel.Receivers, task, partaker) == ChatRoomKinds.Shine)
                {
                    var members =
                        task.Partakers.Where(p => taskAlarmModel.Receivers.Contains((int) p.Kind))
                            .Select(p => p.Staff)
                            .ToList();

                    members = members.Union(new[] {partaker.Staff}).ToList();

                    var alarm =
                        TaskAlarmExistsResult.CheckByReceivers(this, taskAlarmModel.TaskId, taskAlarmModel.StaffId,
                            taskAlarmModel.Receivers).TaskAlarm;
                    if (alarm != null && !string.IsNullOrEmpty(alarm.ConversationId))
                    {
                        conversationId = alarm.ConversationId;
                        IMService.ChangeConversationMemberAsync(taskAlarmModel.StaffId.ToString(), conversationId,
                            members.Select(p => p.Id.ToString()).ToArray());
                    }
                    else
                        conversationId = IMService.CreateChatRoom(staff, members, task, taskAlarmModel.AlarmKind);
                }
                else
                { 
                    IMService.AddTaskAlarms(taskAlarmModel.StaffId.ToString(), task.ConversationId).Wait();
                    IMService.ChangeConAttr(taskAlarmModel.StaffId.ToString(), task.ConversationId, "ChatRoomKind", (short)ChatRoomKinds.Tong).Wait();
                }

                this.TaskLogManager.CreateTaskLog(taskAlarmModel.TaskId, taskAlarmModel.StaffId,
                    taskAlarm.GetType().FullName, taskAlarm.Id, ActionKinds.InsertTable,
                    $"创建了一个预警");
            }
            else
            { 
                taskAlarm.ResolveStatus = ResolveStatus.Closed;
                taskAlarm.ResolvedAt = DateTime.Now;  
            }

            taskAlarm.ConversationId = conversationId;

            this.InternalInsert(taskAlarm);
            return taskAlarm;
        }

        public TaskAlarmEntity ChangeResolvedStatus(Guid taskAlarmId, Guid accountId, ResolveStatus newStatus,
            string comment)
        {
            var taskAlarm = TaskAlarmExistsResult.Check(this, taskAlarmId).ThrowIfFailed().TaskAlarm;
            var staff = StaffExistsResult.Check(StaffManager, taskAlarm.Staff.Org.Id, accountId).Staff;

            if (taskAlarm.Staff != staff)
                throw new FineWorkException("预警必须由创建者灭灯。");

            taskAlarm.ResolveStatus = newStatus;

            if(!string.IsNullOrEmpty(comment))
            taskAlarm.Comment = comment;

            if (newStatus == ResolveStatus.Resolved)
                taskAlarm.ResolvedAt = DateTime.Now;
            if (newStatus == ResolveStatus.Closed)
                taskAlarm.ClosedAt = DateTime.Now;

            this.InternalUpdate(taskAlarm);

            var conversationId = taskAlarm.ConversationId;

            if (conversationId != string.Empty)
            {
                if (newStatus == ResolveStatus.Closed)
                    this.IMService.CloseTaskAlarms(staff.Id.ToString(), conversationId).Wait();
                else
                    this.IMService.ResolveTaskAlarms(staff.Id.ToString(), conversationId).Wait();

                var alias = taskAlarm.Task.Partakers.Select(p => p.Staff.Account.PhoneNumber).ToArray();
                var taskId = taskAlarm.Task.Id.ToString();

                //发通知给会议室成员，更新预警
                Task.Factory.StartNew( async() =>
                { 
                    var customisedValue = new Dictionary<string, string>()
                    {
                        ["ConversationId"] = conversationId,
                        ["TaskId"] =taskId,
                        ["StaffId"]=staff.Id.ToString()
                    };
                   await  NotificationManager.SendByAliasAsync("", "", customisedValue, alias);

                    //发布结束会议通知 
                    var imMessage = string.Format(Config["LeanCloud:Messages:Task:CloseTaskAlarm"],staff.Name);
                    await IMService.SendTextMessageByConversationAsync(taskAlarm.Task.Id, accountId, conversationId,"",imMessage);
                }); 
            }  
            return taskAlarm;
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByTaskId(Guid taskId)
        {
            var taskAlarms = this.InternalFetch(p => p.Task.Id == taskId && p.TaskAlarmKind!=TaskAlarmKinds.GreenLight);
            return taskAlarms;
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByResolvedStatus(Guid taskId, Guid staffId, bool isResolved)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task; 

            var taskAlarms =this.InternalFetch(p => p.Task.Id == task.Id && p.TaskAlarmKind!=TaskAlarmKinds.GreenLight);

            taskAlarms = isResolved
                ? taskAlarms.Where(p => p.ResolveStatus == ResolveStatus.Closed).ToList()
                : taskAlarms.Where(p => p.ResolveStatus != ResolveStatus.Closed).ToList();

            return taskAlarms;
        }

        public TaskAlarmEntity FindTaskAlarm(Guid taskAlarmId)
        {
            var taskAlarm = this.InternalFind(taskAlarmId);
            return taskAlarm;
        }

        public TaskAlarmEntity FindTaskAlarmByReceiverKinds(Guid taskId, Guid staffId, int[] receivers)
        {
            var taskAlarm = this.InternalFetch(p => p.Task.Id == taskId && p.Staff.Id == staffId
             && p.TaskAlarmKind!=TaskAlarmKinds.GreenLight).ToList();
            var filterAlarm = taskAlarm.FirstOrDefault(p => !p.ReceiversArray.Except(receivers).Any() && !receivers.Except(p.ReceiversArray).Any());
            return filterAlarm;
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByStaffId(Guid staffId, bool includeGreenLight = false,
            bool includeAllTask = false)
        {
            IEnumerable<TaskAlarmEntity> taskAlarms = null;

            taskAlarms = this.InternalFetch(p => p.Staff.Id == staffId);


            //是否包含与staff相关的所有任务的爆灯
            if (includeAllTask)
            {
                var tasks = TaskManager.FetchTasksByStaffId(staffId)
                    .Select(p => p.Id).ToArray();
                taskAlarms = this.InternalFetch(p => tasks.Contains(p.Task.Id));
            }

            //是否返回绿灯信息
            if (!includeGreenLight)
                return taskAlarms.Where(p => p.TaskAlarmKind != TaskAlarmKinds.GreenLight);

            return taskAlarms;
        }

        public IEnumerable<TaskAlarmEntity> FetchAlarmsWithPartakerByStaffAndKind(Guid staffId,
            TaskAlarmKinds? alarmKind)
        {
            var taskAlarms = this.FetchTaskAlarmsByStaffId(staffId, false, true);
            if (alarmKind != null)
                taskAlarms = taskAlarms.Where(p => p.TaskAlarmKind == alarmKind.Value).ToList();

            return taskAlarms;
        }

        public void UpdateTaskAlarm(TaskAlarmEntity taskAlarm)
        {
            Args.NotNull(taskAlarm, nameof(taskAlarm));
            this.InternalUpdate(taskAlarm);
        }

        public IEnumerable<TaskAlarmEntity> FetchAlarmsByChatRoomKind(Guid taskId, ChatRoomKinds roomKind,
            Guid creatorStaffId)
        {
            //如果会议室类型为tong，则回去协同者和管理者发送的groupname为project的预警 
            var alarmsForTong = this.InternalFetch(p => p.Task.Id == taskId
                                                        && p.ResolveStatus == ResolveStatus.UnResolved
                                                        && ((int) p.TaskAlarmKind).ToString().StartsWith("1")
                                                        && p.Staff.Partakers.Any(a => a.Task.Id == taskId && (
                                                            a.Kind == PartakerKinds.Collaborator ||
                                                            a.Kind == PartakerKinds.Leader)))
                .OrderBy(p => p.CreatedAt);

            if (roomKind == ChatRoomKinds.Tong || roomKind == ChatRoomKinds.Default)
                return alarmsForTong;

            //获取shine会议室的预警
            var allAlarms = this.InternalFetch(p => p.Task.Id == taskId
                                                    && p.ResolveStatus == ResolveStatus.UnResolved);

            return allAlarms.Except(alarmsForTong)
                .Where(p => p.Staff.Id == creatorStaffId).OrderBy(p => p.CreatedAt);
        }

        public IEnumerable<TaskAlarmEntity> FetchAlarmsByConversationId(string conversationId)
        {
            Args.NotEmpty(conversationId, nameof(conversationId));

            var alarms = this.InternalFetch(p => p.ConversationId == conversationId && p.ResolveStatus==ResolveStatus.UnResolved).OrderBy(p=>p.CreatedAt);
            return alarms;
        }


        public IEnumerable<TaskAlarmEntity> FetchAlarmsByPartakerKind(Guid taskId, PartakerKinds partakerKind)
        {
            var kindValue = (int) partakerKind;
            return InternalFetch(p => p.Task.Id == taskId && p.Receivers.Contains(kindValue.ToString()));
        }

        private ChatRoomKinds ChatRoomKindResult(PartakerEntity partaker, TaskAlarmEntity taskAlarm)
        {
            if ((partaker.Kind == PartakerKinds.Collaborator || partaker.Kind == PartakerKinds.Leader)
                && taskAlarm.TaskAlarmKind.GetGroupName() == TaskAlarmFactor.Project)
                return ChatRoomKinds.Tong;
            return ChatRoomKinds.Shine;
        }

        private string FindConversationId(Guid staffId, TaskAlarmEntity taskAlarm)
        {
            var conversationId = string.Empty;
            var partaker = PartakerExistsResult.CheckForStaff(taskAlarm.Task, staffId).Partaker;
            if (this.ChatRoomKindResult(partaker, taskAlarm) == ChatRoomKinds.Tong)
                conversationId = taskAlarm.Task.ConversationId;
            else
            {
                var creatorClient = new AVIMClient(staffId.ToString());
                var query = creatorClient.GetQuery();
                var existsConversation = (query.WhereEqualTo("attr.TaskId", taskAlarm.Task.Id.ToString())
                    .WhereEqualTo("attr.ChatRoomKind", (int)ChatRoomKinds.Shine)
                    .WhereEqualTo("attr.Creator", staffId.ToString()).FindAsync().Result)
                    .FirstOrDefault();
                if (existsConversation != null)
                    conversationId = existsConversation.ConversationId; 
            }
            return conversationId; 
        }

        private int[] GetPartakerValues()
        {

            var partakerKinds = ((int[])Enum.GetValues(typeof(PartakerKinds)))
                .Where(p => p != (int)PartakerKinds.Unspecified);

            return partakerKinds.ToArray();
        }

        private ChatRoomKinds GetRoomKindByReceivers(int[] receivers,TaskEntity task, PartakerEntity partaker)
        {

            if (!receivers.Any()) return ChatRoomKinds.Default;

            var partakerKinds = GetPartakerValues();
            //全选进tong
            if (!partakerKinds.Except(receivers).Any()) return ChatRoomKinds.Tong;


            //非全选的情况下判断与当前用户组成的会议室是否和tong会议室成员一致
            var allPartakers = task.Partakers.ToList();
            var receiverPartakers = allPartakers.Where(p => receivers.Contains((int)p.Kind)).ToList();
            var partakerWithoutCreator = receiverPartakers.Where(p => p.Id != partaker.Id).ToList();

            //不能与自己形成聊天室
            if (receiverPartakers.Count() == 1 && receiverPartakers.First().Id == partaker.Id)
                throw new FineWorkException("选择的沟通角色为自己，请重新选择."); 
          

            receivers.ToList().Add((int) partaker.Kind);
            if (receivers.Distinct().Count() == partakerKinds.Count())
            {
                partakerWithoutCreator.Add(partaker);
                if (partakerWithoutCreator.Count() == allPartakers.Count()) return ChatRoomKinds.Tong; 
            }

            return ChatRoomKinds.Shine; 
        }


        public void UpdateRemoteServer()
        {
            var alarms = this.InternalFetch(p => p.ConversationId == null && p.TaskAlarmKind != 0).ToList();

            alarms.ForEach(p =>
            {
                var partaker = PartakerExistsResult.CheckForStaff(p.Task, p.Staff.Id).Partaker;

                var conversationId = "";
                var receiver = "";

                if(partaker!=null)
                    conversationId=FindConversationId(p.Staff.Id, p); 


                if (partaker!=null && partaker.Kind == PartakerKinds.Leader && p.TaskAlarmKind.GetGroupName() != TaskAlarmFactor.Project)
                {
                    receiver = ((int) PartakerKinds.Mentor).ToString();
                }
                if (partaker != null &&  partaker.Kind == PartakerKinds.Mentor)
                {
                    receiver = ((int) PartakerKinds.Leader).ToString();
                }
                if (partaker != null &&  partaker.Kind == PartakerKinds.Collaborator &&
                    p.TaskAlarmKind.GetGroupName() != TaskAlarmFactor.Project)
                {
                    receiver = ((int) PartakerKinds.Leader).ToString();
                }
                if (partaker != null && partaker.Kind == PartakerKinds.Recipient)
                {
                    receiver = ((int) PartakerKinds.Leader).ToString();
                }
                if(partaker != null)
                {
                    p.Receivers = receiver;
                    p.ConversationId = conversationId;
                    InternalUpdate(p);
                }
             
            });
        }

    }
}