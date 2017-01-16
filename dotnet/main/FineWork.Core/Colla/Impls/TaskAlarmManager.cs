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
            IConfiguration config,
            IConversationManager conversationManager,
            IMemberManager memberManager)
            : base(dbContextProvider)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (taskLogManager == null) throw new ArgumentNullException(nameof(taskLogManager));
            if (imService == null) throw new ArgumentException(nameof(imService));
            if (config == null) throw new ArgumentException(nameof(config));
            if (conversationManager == null) throw new ArgumentException(nameof(conversationManager));

            this.m_TaskManager = taskManager;
            this.m_StaffManager = staffManager;
            this.m_PartakerManager = partakerManager;
            this.m_TaskLogManager = taskLogManager;
            this.m_IMService = imService;
            this.m_NotificationManager = notificatioinManager;
            this.m_Config = config;
            m_ConversationManager = conversationManager;
            m_MemberManager = memberManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IIMService m_IMService;
        private readonly INotificationManager m_NotificationManager;
        private readonly IConfiguration m_Config;
        private readonly IConversationManager m_ConversationManager;
        private readonly IMemberManager m_MemberManager;

        public TaskAlarmEntity CreateTaskAlarm(CreateTaskAlarmModel taskAlarmModel)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskAlarmModel.TaskId).ThrowIfFailed().Task;

            if(task.Progress==100)
                throw  new FineWorkException("任务已完成，不可以爆灯.");
            if(task.IsDeserted.HasValue && task.IsDeserted.Value)
                throw new FineWorkException("任务已被终止，不可以爆灯.");

            var staff = StaffExistsResult.Check(this.m_StaffManager, taskAlarmModel.CreatorId).ThrowIfFailed().Staff;
            PartakerExistsResult.CheckForStaff(task, taskAlarmModel.CreatorId).ThrowIfFailed();
            var conversation = task.Conversation;
            List<StaffEntity> members = new List<StaffEntity>();
            taskAlarmModel.PartakerKinds?.ToList().ForEach(p =>
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
            taskAlarm.AlarmId = taskAlarmModel.AlarmId;

            //自定义预警
            if (taskAlarmModel.AlarmKind == TaskAlarmKinds.Custom)
            {
                taskAlarm.AlarmDesc = taskAlarmModel.AlarmDesc;
                taskAlarm.TaskAlarmKind = TaskAlarmKinds.Custom;
            }

            //非绿灯的情况下创建聊天室
            if (taskAlarmModel.AlarmKind != TaskAlarmKinds.GreenLight)
            {
                var roomInfo = CreateChatRoom(taskAlarmModel, task, staff);
                conversation = roomInfo;
                this.m_TaskLogManager.CreateTaskLog(taskAlarmModel.TaskId, taskAlarmModel.CreatorId,
                    taskAlarm.GetType().FullName, taskAlarm.Id, ActionKinds.InsertTable,
                    $"创建了一个预警");
            }
            else
            {
                taskAlarm.ResolveStatus = ResolveStatus.Closed;
                taskAlarm.ResolvedAt = DateTime.Now;
            }

            taskAlarm.Conversation = conversation;
            this.InternalInsert(taskAlarm);
            return taskAlarm;
        }

        private ConversationEntity CreateChatRoom(CreateTaskAlarmModel taskAlarmModel, TaskEntity task,
            StaffEntity staff)
        {
            var convr = task.Conversation;
            List<StaffEntity> conversationMembers = new List<StaffEntity>();

            if (taskAlarmModel.PartakerKinds != null && taskAlarmModel.PartakerKinds.Any())
                foreach (var receiverKind in taskAlarmModel.PartakerKinds)
                {
                    var staffs =
                        m_PartakerManager.FetchPartakerByKind(task.Id, (PartakerKinds) receiverKind)
                            .Select(p => p.Staff)
                            .ToList();
                    conversationMembers.AddRange(staffs);
                }
            if (taskAlarmModel.StaffIds != null && taskAlarmModel.StaffIds.Any())
            {
                var staffs = m_StaffManager.FetchStaffsByIds(taskAlarmModel.StaffIds).ToList();
                conversationMembers = conversationMembers.Union(staffs).ToList();
            }

            conversationMembers = conversationMembers.Union(new[] {staff}).ToList();

            var memberIds = conversationMembers.Select(p => p.Id).ToArray();
            if (this.GetChatRoomKindsByStaffs(memberIds, task, staff) == ChatRoomKinds.Shine)
            {
                var existsConversationId = m_ConversationManager.FindConversationByStaffIds(task.Id, memberIds);

                if (existsConversationId != null)
                    convr = existsConversationId;
                else
                {
                    var conversationId = m_IMService.CreateChatRoom(staff, conversationMembers, task,
                        taskAlarmModel.AlarmKind);
                    convr = m_ConversationManager.CreateConversation(conversationId);

                    //为staff创建alarm的对应关系 
                    m_MemberManager.CreateMember(convr.Id, memberIds);
                }
            }
            else
            {
                m_IMService.AddTaskAlarms(taskAlarmModel.CreatorId.ToString(), task.Conversation.Id).Wait();
                m_IMService.ChangeConAttrAsync(taskAlarmModel.CreatorId.ToString(), task.Conversation.Id, "ChatRoomKind",
                    (short) ChatRoomKinds.Tong).Wait();
            }
            return convr;
        }

        public TaskAlarmEntity ChangeResolvedStatus(Guid taskAlarmId, Guid accountId, ResolveStatus newStatus,
            string comment)
        {
            var taskAlarm = TaskAlarmExistsResult.Check(this, taskAlarmId).ThrowIfFailed().TaskAlarm;
            var staff = StaffExistsResult.Check(m_StaffManager, taskAlarm.Staff.Org.Id, accountId).Staff;

            if (taskAlarm.Staff != staff)
                throw new FineWorkException("预警必须由创建者灭灯。");

            taskAlarm.ResolveStatus = newStatus;

            if (!string.IsNullOrEmpty(comment))
                taskAlarm.Comment = comment;

            if (newStatus == ResolveStatus.Resolved)
                taskAlarm.ResolvedAt = DateTime.Now;
            if (newStatus == ResolveStatus.Closed)
                taskAlarm.ClosedAt = DateTime.Now;

            this.InternalUpdate(taskAlarm);

            var conversationId = taskAlarm.Conversation.Id;

            if (conversationId != string.Empty)
            {
                if (newStatus == ResolveStatus.Closed)
                    this.m_IMService.CloseTaskAlarms(staff.Id.ToString(), conversationId).Wait();
                else
                    this.m_IMService.ResolveTaskAlarms(staff.Id.ToString(), conversationId).Wait();

                var alias = taskAlarm.Task.Partakers.Select(p => p.Staff.Account.PhoneNumber).ToArray();
                var taskId = taskAlarm.Task.Id.ToString();


                #region 
                //发通知给会议室成员，更新预警 
                //var customisedValue = new Dictionary<string, string>()
                //{
                //    ["ConversationId"] = conversationId,
                //    ["TaskId"] = taskId,
                //    ["StaffId"] = staff.Id.ToString()
                //};
                //m_NotificationManager.SendByAliasAsync("", "", customisedValue, alias);
            #endregion


                //发布结束会议通知 
                var imMessage = string.Format(m_Config["LeanCloud:Messages:Task:CloseTaskAlarm"], staff.Name);

                m_IMService.SendTextMessageByConversationAsync(taskAlarm.Task.Id, accountId, conversationId, "",
                    imMessage);

            }
            return taskAlarm;
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByTaskId(Guid taskId)
        {
            var taskAlarms = this.InternalFetch(p => p.Task.Id == taskId && p.TaskAlarmKind != TaskAlarmKinds.GreenLight);
            return taskAlarms;
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByStaffIdWithTaskId(Guid staffId, Guid? taskId)
        {
            var convr = m_ConversationManager.FetchConvertionsByStaffId(staffId);

            if (convr != null)
            {
                var alarms = convr.Select(p => p.TaskAlarms).SelectMany(p => p);
                if (taskId.HasValue)
                    return alarms.Where(p => p.Task.Id == taskId.Value);
                return alarms;
            }

            return new List<TaskAlarmEntity>();
        }

        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByResolvedStatus(Guid taskId, Guid staffId, bool isResolved)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var taskAlarms =
                this.InternalFetch(p => p.Task.Id == task.Id && p.TaskAlarmKind != TaskAlarmKinds.GreenLight);

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


        public IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByCreatorId(Guid staffId, bool includeGreenLight = false,
            bool includeAllTask = false)
        {
            IEnumerable<TaskAlarmEntity> taskAlarms = null;

            taskAlarms =
                this.InternalFetch(p => p.Staff.Id == staffId && p.Task.IsDeserted == null && p.Task.Progress < 100);

            //是否包含与staff相关的所有任务的爆灯
            if (includeAllTask)
            {
                var tasks = m_PartakerManager.FetchPartakersByStaff(staffId).Select(p => p.Task.Id).ToArray();
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
            var taskAlarms = this.FetchTaskAlarmsByCreatorId(staffId, false, true);
            if (alarmKind != null)
                taskAlarms = taskAlarms.Where(p => p.TaskAlarmKind == alarmKind.Value).ToList();

            return taskAlarms;
        }

        public void UpdateTaskAlarm(TaskAlarmEntity taskAlarm)
        {
            Args.NotNull(taskAlarm, nameof(taskAlarm));
            this.InternalUpdate(taskAlarm);
        }

        public IEnumerable<TaskAlarmEntity> FetchAlarmsByConversationId(string conversationId)
        {
            Args.NotEmpty(conversationId, nameof(conversationId));

            var alarms =
                this.InternalFetch(
                    p => p.Conversation.Id == conversationId && p.ResolveStatus == ResolveStatus.UnResolved)
                    .OrderBy(p => p.CreatedAt);
            return alarms;
        }


        private ChatRoomKinds GetChatRoomKindsByStaffs(Guid[] staffIds, TaskEntity task, StaffEntity staff)
        {
            if (!staffIds.Any()) return ChatRoomKinds.Default;

            var allPartakersInTask = m_PartakerManager.FetchPartakersByTask(task.Id).Select(p => p.Staff.Id).ToList();

            if (!allPartakersInTask.Except(staffIds).Any()) return ChatRoomKinds.Tong;

            //不能与自己形成聊天室
            if (staffIds.Count() == 1 && staffIds.First() == staff.Id)
                throw new FineWorkException("选择的沟通角色只有自己，请重新选择.");

            return ChatRoomKinds.Shine;
        }


    }
}