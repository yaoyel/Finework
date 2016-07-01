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
            IConfiguration config)
            : base(dbContextProvider)
        {
            if (orgManagerResolver == null) throw new ArgumentNullException(nameof(orgManagerResolver));
            if (staffManagerResolver == null) throw new ArgumentNullException(nameof(staffManagerResolver));
            if (imService == null) throw new ArgumentNullException(nameof(imService));
            if (taskIncentiveManagerResolver == null) throw new ArgumentNullException(nameof(taskIncentiveManagerResolver));
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
            Config = config;
        }

        private ILazyResolver<IOrgManager> OrgManagerResolver { get;  }

        private ILazyResolver<IStaffManager> StaffManagerResolver { get;  } 

        private  IIMService IMService { get;}

        private ILazyResolver<ITaskIncentiveManager> TaskIncentiveManagerResolver { get; }

        private ILazyResolver<IAlarmManager> AlarmManagerResolver { get;}

        private ILazyResolver<IPartakerManager> PartakerManagerResolver { get; }

        private ILazyResolver<IPartakerInvManager> PartakerInvManagerResolver { get; }

        private IConfiguration Config { get; }

        private  IOrgManager OrgManager {
            get { return OrgManagerResolver.Required; }
        }

        private  IStaffManager StaffManager {
            get { return StaffManagerResolver.Required; }
        } 

        private ITaskIncentiveManager TaskIncentiveManager {
            get { return TaskIncentiveManagerResolver.Required; }
        }

        private  IAlarmManager AlarmManager {
            get { return AlarmManagerResolver.Required; }
        }

        private  IPartakerManager  PartakerManager {
            get { return PartakerManagerResolver.Required; }
        }

        private IPartakerInvManager PartakerInvManager
        {
            get { return this.PartakerInvManagerResolver.Required; }
        }

        public TaskEntity CreateTask(CreateTaskModel taskModel)
        {
           
            var staff = StaffExistsResult.Check(this.StaffManager, taskModel.CreatorStaffId).ThrowIfFailed().Staff;
            var parentTask = taskModel.ParentTaskId.HasValue
                ? TaskExistsResult.Check(this, taskModel.ParentTaskId.Value).ThrowIfFailed().Task
                : null;
            if (taskModel.IsRecruitEnabled &&(taskModel.RecruitmentRoles==null || !taskModel.RecruitmentRoles.Any()))
                throw new FineWorkException("请设置任务招募的角色。");

            var clientIds = new List<string>();
            var leader= staff;
            var members = new List<StaffEntity>();
            if(taskModel.LeaderStaffId.HasValue)
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
            };

            if (taskModel.IsRecruitEnabled)
            {
                task.IsRecruitEnabled = taskModel.IsRecruitEnabled;
                task.RecruitmentDesc = taskModel.RecruitmentDesc;
                task.RecruitmentRoles = string.Join(",", taskModel.RecruitmentRoles);
            }
            //创建任务的人自动成为任务的负责人
            //如果创建任务是LeaderStaffId不为空，则负责人为改字段值，用户创建子任务
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

            //创建定时预警
            if (taskModel.Alarms != null)
                taskModel.Alarms.ForEach(p =>
                {
                AlarmManager.CreateAlarmPeriod(task, p.Weekdays, p.ShortTime, p.Bell,p.IsEnabled);
                }); 

            //添加任务成员
            //添加协同者
            if (taskModel.Collaborators != null)
                taskModel.Collaborators.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff,PartakerKinds.Collaborator);
                    members.Add(inviteeStaff);
                });

            //添加指导者
            if (taskModel.Mentors != null)
                taskModel.Mentors.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff, PartakerKinds.Mentor); 
                    members.Add(inviteeStaff);
                });

            //添加接受者
            if (taskModel.Recipients != null)
                taskModel.Recipients.ForEach(p =>
                {
                    var inviteeStaff = StaffExistsResult.Check(this.StaffManager, p).Staff;
                    PartakerInvManager.QuickAdd(task, staff, inviteeStaff, PartakerKinds.Recipient);
                    members.Add(inviteeStaff);
                });

             CreateConversationByTask(task, leader, clientIds).Wait(); 

            if (members.Any())
            { 
                IMService.AddMemberAsync(leader.Id.ToString(), task.ConversationId,members.Select(p=>p.Id.ToString()).ToArray());

                var memberNames = string.Join(",", members.Select(p => p.Name));
                var message = string.Format(Config["LeanCloud:Messages:Task:PartakerInv"], staff.Name, memberNames, task.Name);
                IMService.SendTextMessageByConversationAsync(task.Id,staff.Account.Id,task.ConversationId,task.Name, message);
            }

            return task;
        }

        public TaskEntity FindTask(Guid taskId)
        {
            return this.InternalFind(taskId);
        }

        public TaskEntity FindTaskByNameInOrg(Guid orgId, string taskName)
        {
            return this.InternalFetch(x => x.Creator.Org.Id == orgId && x.Name == taskName).SingleOrDefault();
        }

        public TaskEntity FindTaskByNameInParent(Guid parentTaskId, string taskName)
        {
            return this.InternalFetch(x => x.ParentTask.Id == parentTaskId && x.Name == taskName).SingleOrDefault();
        }

        public IEnumerable<TaskEntity> FetchTasksByStaff(params Guid[] ids)
        {
            return this.InternalFetch(x => ids.Contains(x.Creator.Id));
        }

        public IEnumerable<TaskEntity> FetchTasksByStaffId(Guid staffId)
        { 
            var context = this.Session.DbContext;
            var set = context.Set<PartakerEntity>().AsNoTracking();  
            var tasks = set.Where(p => p.Staff.Id==staffId) 
                .Select(p=>p.Task)
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
                .Include(p => p.Alarms.Select(s => s.Staff.Org));

            return tasks;
        }

        public IEnumerable<TaskEntity> FetchTasksByOrgId(Guid orgId)
        { 
            var set = this.Session.DbContext.Set<TaskEntity>().AsNoTracking();
            var tasks = set.Where(p => p.Creator.Org.Id == orgId)
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
                .Include(p => p.Alarms.Select(s => s.Staff.Org));

            return tasks.ToList();

        }

        public int FetchTaskNumByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Partakers.Any(a => a.Staff.Id == staffId)).Count();
        }

        public void UpdateTask(TaskEntity task)
        {
            Args.NotNull(task, nameof(task));
            this.InternalUpdate(task);
        }

        public void ChangeParentTask(Guid accountId,Guid taskId,Guid? parentTaskId)
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
                IMService.SendTextMessageByConversationAsync(parentTask.Id,accountId,parentTask.ConversationId,parentTask.Name, message)
                    .Wait();

                //通知迁出方
                if (originalParentTask != null)
                {
                    message = $"子任务 {task.Name} 已迁出";
                    IMService.SendTextMessageByConversationAsync(originalParentTask.Id,accountId,originalParentTask.ConversationId, originalParentTask.Name, message);
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
                IMService.SendTextMessageByConversationAsync(accountId,task.Id,task.ConversationId,task.Name, message);

            } 

            this.InternalUpdate(task);
        }

        private async Task CreateConversationByTask(TaskEntity task, StaffEntity staff,IList<string> clientIds)
        {
            Args.NotNull(task, nameof(task));
            Args.NotNull(staff, nameof(staff)); 

            //将任务信息放入聊天室的属性中
            var attrs = new Dictionary<string, object>()
            {
                ["ChatRoomKind"] = (short)ChatRoomKinds.Default,
                ["TaskId"] = task.Id.ToString(),
                ["TaskName"] = task.Name,
                ["Progress"] = task.Progress,
                ["Creator"] = task.Creator.Id.ToString(),
                ["LeaderStaffId"] = task.Partakers.First(p => p.Kind == PartakerKinds.Leader).Staff.Id.ToString(),
                ["AlarmsCount"]=0,
                ["ResolvedCount"] =0 
            };
            var conversationId =
                await this.IMService.CreateConversationAsync(staff.Id.ToString(), clientIds, task.Name, attrs);
            task.ConversationId = conversationId;
            await this.InternalUpdateAsync(task);
        }

    }
}