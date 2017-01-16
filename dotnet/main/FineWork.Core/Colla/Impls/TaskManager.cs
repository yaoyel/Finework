using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class TaskManager : AefEntityManager<TaskEntity, Guid>, ITaskManager
    {
        public TaskManager(ISessionProvider<AefSession> dbContextProvider, ILazyResolver<IOrgManager> orgManagerResolver,
            ILazyResolver<IStaffManager> staffManagerResolver,
            IIMService imService,
            ILazyResolver<ITaskIncentiveManager> taskIncentiveManagerResolver,
            ILazyResolver<IAlarmManager> alarmManagerResolver,
            ILazyResolver<IPartakerManager> partakerManagerResolver,
            ILazyResolver<IPartakerInvManager> partakerInvManagerResolver,
            ILazyResolver<IAnnouncementManager> anncResolver,
            ILazyResolver<ITaskTempManager> taskTempResolver, 
            IConfiguration config,
            IConversationManager conversationManager,
            IMemberManager memberManager )
            : base(dbContextProvider)
        {
            if (orgManagerResolver == null) throw new ArgumentNullException(nameof(orgManagerResolver));
            if (staffManagerResolver == null) throw new ArgumentNullException(nameof(staffManagerResolver));
            if (imService == null) throw new ArgumentNullException(nameof(imService));
            if (taskIncentiveManagerResolver == null)
                throw new ArgumentNullException(nameof(taskIncentiveManagerResolver));
            if (alarmManagerResolver == null) throw new ArgumentNullException(nameof(alarmManagerResolver));
            if (partakerManagerResolver == null) throw new ArgumentNullException(nameof(partakerManagerResolver));
            if (partakerInvManagerResolver == null) throw new ArgumentNullException(nameof(partakerInvManagerResolver));
            this.OrgManagerResolver = orgManagerResolver;
            this.StaffManagerResolver = staffManagerResolver;
            this.IMService = imService;
            this.TaskIncentiveManagerResolver = taskIncentiveManagerResolver;
            this.AlarmManagerResolver = alarmManagerResolver;
            this.PartakerManagerResolver = partakerManagerResolver;
            PartakerInvManagerResolver = partakerInvManagerResolver;
            AnncResolver = anncResolver;
            TaskTempResolver = taskTempResolver;
            Config = config;
            m_ConversationManager = conversationManager;
            m_MemberManager = memberManager; 
        }

        private ILazyResolver<IOrgManager> OrgManagerResolver { get; }
        private ILazyResolver<IStaffManager> StaffManagerResolver { get; }
        private IIMService IMService { get; }
        private ILazyResolver<ITaskIncentiveManager> TaskIncentiveManagerResolver { get; }
        private ILazyResolver<IAlarmManager> AlarmManagerResolver { get; }
        private ILazyResolver<IPartakerManager> PartakerManagerResolver { get; }
        private ILazyResolver<IPartakerInvManager> PartakerInvManagerResolver { get; }
        private ILazyResolver<IAnnouncementManager> AnncResolver { get; }
        private ILazyResolver<ITaskTempManager> TaskTempResolver { get; }
        private IConfiguration Config { get; }
        private readonly IConversationManager m_ConversationManager;
        private readonly IMemberManager m_MemberManager; 


        private IOrgManager OrgManager
        {
            get { return OrgManagerResolver.Required; }
        }

        private IStaffManager StaffManager
        {
            get { return StaffManagerResolver.Required; }
        }

        private ITaskIncentiveManager TaskIncentiveManager
        {
            get { return TaskIncentiveManagerResolver.Required; }
        }

        private IAlarmManager AlarmManager
        {
            get { return AlarmManagerResolver.Required; }
        }

        private IPartakerManager PartakerManager
        {
            get { return PartakerManagerResolver.Required; }
        }

        private IPartakerInvManager PartakerInvManager
        {
            get { return this.PartakerInvManagerResolver.Required; }
        }

        private IAnnouncementManager AnncManager
        {
            get { return this.AnncResolver.Required; }
        }

        private ITaskTempManager TaskTempManager
        {
            get { return this.TaskTempResolver.Required; }
        }

        public TaskEntity CreateTask(CreateTaskModel taskModel)
        {
            var staff = StaffExistsResult.Check(this.StaffManager, taskModel.CreatorStaffId).ThrowIfFailed().Staff;
            var parentTask = taskModel.ParentTaskId.HasValue
                ? TaskExistsResult.Check(this, taskModel.ParentTaskId.Value).ThrowIfFailed().Task
                : null;

            if (parentTask != null && parentTask.Progress == 100)
                throw new FineWorkException("上级任务已完成，请重新选择。");
            if (taskModel.IsRecruitEnabled && (taskModel.RecruitmentRoles == null || !taskModel.RecruitmentRoles.Any()))
                throw new FineWorkException("请设置任务招募的角色.");

            var clientIds = new List<string>();
            var leader = staff;
            var members = new List<StaffEntity>();
            if (taskModel.LeaderStaffId.HasValue)
                leader = StaffExistsResult.Check(this.StaffManager, taskModel.LeaderStaffId.Value).ThrowIfFailed().Staff;

            var task = new TaskEntity()
            {
                Id = Guid.NewGuid(),
                Name = taskModel.Name,
                EndAt = taskModel.EndAt,
                Creator = staff,
                ParentTask = parentTask,
                Goal = taskModel.Goal,
                Level = taskModel.Level,
                IsCollabratorInvEnabled = taskModel.IsCollabratorInvEnabled,
                IsMentorInvEnabled = taskModel.IsMentorInvEnabled,
                CopyFrom=taskModel.CopyFrom
            };

            if (taskModel.IsRecruitEnabled)
            {
                task.IsRecruitEnabled = taskModel.IsRecruitEnabled;
                task.RecruitmentDesc = taskModel.RecruitmentDesc;
                task.RecruitmentRoles = string.Join(",", taskModel.RecruitmentRoles);
            }
            //创建任务的人自动成为任务的负责人
            //如果创建任务是LeaderStaffId不为空，则负责人为该字段值，用于创建子任务
            task.Partakers.Add(new PartakerEntity
            {
                Id = Guid.NewGuid(),
                Task = task,
                Staff = leader,
                Kind = PartakerKinds.Leader,
                CreatedAt = DateTime.Now
            });

            this.InternalInsert(task);

            //将leader加入client
            clientIds.Add(leader.Id.ToString());

            //创建激励
            if (taskModel.Incentives != null)
                taskModel.Incentives.ForEach(p =>
                {
                    TaskIncentiveManager.UpdateTaskIncentive(task.Id, p.IncentiveKindId, p.Amount);
                });

            // 创建定时预警
            if (taskModel.Alarms != null)
                taskModel.Alarms.ForEach(p =>
                {
                    p.TaskId = task.Id;
                    AlarmManager.CreateAlarmPeriod(p);
                });

            
            //if (taskModel.AlarmTempIds != null && taskModel.AlarmTempIds.Any())
            //{
            //    taskModel.AlarmTempIds.ForEach(p =>
            //    {
            //        AlarmManager.CreateAlarmPeriodByTemp(p); 
            //    });
            //}

            //添加任务成员 
            if (taskModel.Collaborators != null)
            {
                taskModel.Collaborators.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff, PartakerKinds.Collaborator);
                    members.Add(inviteeStaff);
                });
            }


            //添加指导者
            if (taskModel.Mentors != null)
            {
                taskModel.Mentors.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff, PartakerKinds.Mentor);
                    members.Add(inviteeStaff);
                });
            }

            //添加接受者
            if (taskModel.Recipients != null)
            {
                taskModel.Recipients.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff, PartakerKinds.Recipient);
                    members.Add(inviteeStaff);
                });
            }

            CreateConversationByTask(task, leader, clientIds).Wait();
            members.Add(staff);
            m_MemberManager.CreateMember(task.ConversationId, members.Select(p => p.Id).ToArray());
            if (members.Any() && members.Count() > 1)
            {
                var conMember = members.Select(p => p.Id.ToString()).ToArray();
                IMService.AddMemberAsync(leader.Id.ToString(), task.Conversation.Id, conMember);

                var memberNames = string.Join(",", members.Where(p => p.Id != staff.Id).Select(p => p.Name));
                var message = string.Format(Config["LeanCloud:Messages:Task:PartakerInv"], staff.Name, memberNames,
                    task.Name);
                IMService.SendTextMessageByConversationAsync(task.Id, staff.Account.Id, task.Conversation.Id, task.Name,
                    message);
            }

            return task;
        }

        public TaskEntity FindTask(Guid taskId)
        {
            return this.InternalFind(taskId);
        }

        public IEnumerable<TaskEntity> FetchTaskByName(Guid orgId, string name)
        {
            return
                this.InternalFetch(
                    p =>
                        p.Creator.Org.Id == orgId &&
                        (p.Name.Contains(name) || p.Partakers.Any(a => a.Staff.Name.Contains(name))));
        }


        public TaskEntity FindTaskByNameInOrg(Guid orgId, string taskName)
        {
            return
                this.InternalFetch(x => x.Creator.Org.Id == orgId && x.Name == taskName && x.IsDeserted == null)
                    .SingleOrDefault();
        }

        public TaskEntity FindTaskByNameInParent(Guid parentTaskId, string taskName)
        {
            return
                this.InternalFetch(x => x.ParentTask.Id == parentTaskId && x.Name == taskName && x.IsDeserted == null)
                    .SingleOrDefault();
        }

        public IEnumerable<TaskEntity> FetchTasksByStaff(params Guid[] ids)
        {
            return this.InternalFetch(x => ids.Contains(x.Creator.Id) && x.IsDeserted == null);
        }

        public IEnumerable<TaskEntity> FetchTasksByStaffId(Guid staffId, bool isLazyload = true)
        {
            var context = this.Session.DbContext;
            var set = context.Set<PartakerEntity>().AsNoTracking();

            var tasks = set.Where(p => p.Staff.Id == staffId && p.Task.Progress < 100 && p.Task.IsDeserted == null)
                .Select(p => p.Task)
                .Include(p => p.Creator.Account)
                .Include(p => p.Creator.Org)
                .Include(p => p.Partakers.Select(s => s.Staff.Org))
                .Include(p => p.Partakers.Select(s => s.Staff.Account))
                .Include(p => p.Incentives.Select(s => s.IncentiveKind))
                .Include(p => p.ChildTasks.Select(s => s.Creator.Account))
                .Include(p => p.ChildTasks.Select(s => s.Creator.Org))
                .Include(p => p.ParentTask)
                .Include(p => p.Newses.Select(s => s.Staff.Org))
                .Include(p => p.Newses.Select(s => s.Staff.Account))
                .Include(p => p.Alarms.Select(s => s.Staff.Account))
                .Include(p => p.Alarms.Select(s => s.Staff.Org))
                .Include(p => p.Conversation)
                .Include(p => p.ParentTask.Conversation);

            return tasks;
        }

        public IEnumerable<TaskEntity> FetchTasksByOrgId(Guid orgId, bool includeEnd = false, bool includeAll = true)
        {
            var set = this.Session.DbContext.Set<TaskEntity>().AsNoTracking();

            var tasks = set.Where(p => p.Creator.Org.Id == orgId && p.IsDeserted == null)
                .Include(p => p.Creator.Account)
                .Include(p => p.Creator.Org)
                .Include(p => p.Partakers.Select(s => s.Staff.Org))
                .Include(p => p.Partakers.Select(s => s.Staff.Account));

            if (!includeEnd)
                tasks = tasks.Where(p => p.Report == null);
            if (includeAll)
                tasks = tasks
                    .Include(p => p.Incentives.Select(s => s.IncentiveKind))
                    .Include(p => p.ChildTasks.Select(s => s.Creator.Account))
                    .Include(p => p.ChildTasks.Select(s => s.Creator.Org))
                    .Include(p => p.ParentTask)
                    .Include(p => p.Newses.Select(s => s.Staff.Org))
                    .Include(p => p.Newses.Select(s => s.Staff.Account))
                    .Include(p => p.Alarms.Select(s => s.Staff.Account))
                    .Include(p => p.Alarms.Select(s => s.Staff.Org))
                    .Include(p => p.Conversation.Members)
                    .Include(p => p.Conversation.TaskAlarms)
                    .Include(p => p.ParentTask.Conversation);

            return tasks.ToList();

        }

        public int FetchTaskNumByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Partakers.Any(a => a.Staff.Id == staffId && p.IsDeserted == null)).Count();
        }

        public void UpdateTask(TaskEntity task)
        {
            Args.NotNull(task, nameof(task));
            this.InternalUpdate(task);
        } 

        public TaskEntity FindByConvrId(string convrId)
        {
            return this.InternalFetch(p => p.Conversation.Id == convrId).FirstOrDefault();
        }

        public void ChangeParentTask(Guid accountId, Guid taskId, Guid? parentTaskId)
        {
            var task = TaskExistsResult.Check(this, taskId).ThrowIfFailed().Task;
            var originalParentTask = task.ParentTask;
            string message; 

            if (parentTaskId.HasValue)
            {
                var parentTask = TaskExistsResult.Check(this, parentTaskId.Value).ThrowIfFailed().Task;
                task.ParentTask = parentTask;
                //通知迁入方
                message = $"子任务 {task.Name} 已迁入";
                IMService.SendTextMessageByConversationAsync(parentTask.Id, accountId, parentTask.Conversation.Id,
                    parentTask.Name, message)
                    .Wait();

                //通知迁出方
                if (originalParentTask != null)
                {
                    message = $"子任务 {task.Name} 已迁出";
                    IMService.SendTextMessageByConversationAsync(originalParentTask.Id, accountId,
                        originalParentTask.Conversation.Id, originalParentTask.Name, message);
                }

                ////通知本任务
                //message = $"本任务已迁入{parentTask.Name}";
                //IMService.SendTextMessageByConversationAsync( task.ConversationId,message);
            }
            else
            {
                task.ParentTask = null;
                //创建一条迁出的消息
                message = $"任务从 {originalParentTask.Name} 迁出";
                IMService.SendTextMessageByConversationAsync(accountId, task.Id, task.Conversation.Id, task.Name,
                    message); 
            }

            this.InternalUpdate(task);
        }

        private async Task CreateConversationByTask(TaskEntity task, StaffEntity staff, IList<string> clientIds)
        {
            Args.NotNull(task, nameof(task));
            Args.NotNull(staff, nameof(staff));

            //将任务信息放入聊天室的属性中
            var attrs = new Dictionary<string, object>()
            {
                ["ChatRoomKind"] = (short) ChatRoomKinds.Default,
                ["TaskId"] = task.Id.ToString(),
                ["TaskName"] = task.Name,
                ["Progress"] = task.Progress,
                ["Creator"] = task.Creator.Id.ToString(),
                ["LeaderStaffId"] = task.Partakers.First(p => p.Kind == PartakerKinds.Leader).Staff.Id.ToString(),
                ["AlarmsCount"] = 0,
                ["ResolvedCount"] = 0,
                ["IsEnd"] = false,
                ["OrgId"] = staff.Org.Id.ToString()
            };
            var conversationId =
                await this.IMService.CreateConversationAsync(staff.Id.ToString(), clientIds, task.Name, attrs); 

            var convr = m_ConversationManager.CreateConversation(conversationId);

            task.Conversation = convr;
            await this.InternalUpdateAsync(task);
        }

        public void ShareTask(Guid taskId, Guid staffId)
        {
            var taskTemp = TaskTempExistsResult.CheckForTask(this.TaskTempManager, taskId).TaskTemp;

            if (taskTemp == null)
                TaskTempManager.CreateTaskTemp(taskId, staffId);
            else
            {
                taskTemp.LastUpdatedAt = DateTime.Now;

                TaskTempManager.UpdateTaskTemp(taskTemp);
            }
        }

        //public TaskEntity CreateTask(CreateTaskOnSharedModel model)
        //{
        //    var task = TaskExistsResult.Check(this, model.SharedTaskId).ThrowIfFailed().Task;
        //    var creator = StaffExistsResult.Check(this.StaffManager, model.CreatorStaffId).ThrowIfFailed().Staff;
        //    var collaborators = new List<Guid>();

        //    var taskTemp = TaskTempExistsResult.CheckForTask(this.TaskTempManager, task.Id).TaskTemp;

        //    if (taskTemp == null)
        //        throw new FineWorkException("该任务未被分享！");
        //    var parentTask = model.ParentTaskId.HasValue
        //        ? TaskExistsResult.Check(this, model.ParentTaskId.Value).Task
        //        : null;

        //    var newTask = new TaskEntity()
        //    {
        //        Id = Guid.NewGuid(),
        //        ParentTask = parentTask,
        //        Creator = creator,
        //        Name = model.Name,
        //        CopyFrom = taskTemp.Task.Id
        //    };

        //    this.InternalInsert(newTask);

        //    PartakerManager.CreatePartaker(newTask.Id, creator.Id, PartakerKinds.Leader, false);


        //    collaborators.AddRange(model.Anncs.Select(p => p.InspecterId).Distinct());
        //    collaborators.AddRange(model.Anncs.SelectMany(p => p.ExecutorIds).Distinct());
        //    collaborators = collaborators.Where(p => p != creator.Id).Distinct().ToList();

        //    if (collaborators.Any())
        //    {
        //        collaborators.ForEach(p =>
        //        {
        //            PartakerManager.CreateCollabrator(newTask.Id, p);
        //        });
        //    }

        //    var leader = newTask.Partakers.First(p => p.Kind == PartakerKinds.Leader);
        //    var members = newTask.Partakers.Select(p => p.Staff).ToList();

        //    //创建一个聊天室  
        //    CreateConversationByTask(newTask, creator, new List<string>() {creator.Id.ToString()}).Wait();

        //    if (members.Any() && members.Count() > 1)
        //    {
        //        var conMember = members.Select(p => p.Id.ToString()).ToArray();
        //        IMService.AddMemberAsync(leader.Id.ToString(), newTask.Conversation.Id, conMember);

        //        var memberNames = string.Join(",", members.Where(p => p.Id != creator.Id).Select(p => p.Name));
        //        var message = string.Format(Config["LeanCloud:Messages:Task:PartakerInv"], creator.Name, memberNames,
        //            newTask.Name);
        //        IMService.SendTextMessageByConversationAsync(newTask.Id, creator.Account.Id, newTask.Conversation.Id,
        //            newTask.Name,
        //            message);
        //    }

        //    if (model.Anncs.Any())
        //    {
        //        model.Anncs.ForEach(t =>
        //        {
        //            t.TaskId = newTask.Id;
        //            t.CreatorId = model.CreatorStaffId;
        //            t.InspecterId = t.InspecterId == default(Guid) ? model.CreatorStaffId : t.InspecterId;
        //            t.ExecutorIds = t.ExecutorIds.Any() ? t.ExecutorIds : new Guid[] {model.CreatorStaffId};

        //            AnncManager.CreateAnnc(t);
        //        });

        //    }
        //    return newTask;
        //}

        public int CountCopysBySharedTaskId(Guid sharedTaskId)
        {
            return this.InternalFetch(p => p.CopyFrom != null && p.CopyFrom == sharedTaskId).Count;
        }


        public void DeleteTask(Guid taskId)
        {
            var task = TaskExistsResult.Check(this, taskId).Task;

            if(task==null) return;
            
            //目前仅在根据任务模板创建任务返回的时候需要删除任务，此时只需要删除 定时预警，成员，激励,计划和任务本身
            TaskIncentiveManager.DeleteTaskIncentiveByTaskId(taskId);
            AnncManager.DeleteAnncsByTaskId(taskId);
            AlarmManager.DeleteAlarmPeriodsByTaskId(taskId);
            PartakerManager.DeletePartakersByTaskId(taskId);

            //删除聊天室
            if (task.Conversation != null)
                IMService.RemoveConversationById(task.Conversation.Id);

            this.InternalDelete(task);  
        }
    }
}