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
using FineWork.Common;
using FineWork.Core;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class PartakerManager : AefEntityManager<PartakerEntity, Guid>, IPartakerManager
    {
        public PartakerManager(ISessionProvider<AefSession> dbContextProvider, 
            IStaffManager staffManager, 
            ITaskManager taskManager,
            ILazyResolver<ITaskAlarmManager> taskAlarmManagerResolver,
            IIMService imService ,
            INotificationManager notification,
            IConfiguration config)
            :base(dbContextProvider)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (imService == null) throw new ArgumentNullException(nameof(imService));
            if (taskAlarmManagerResolver == null) throw new ArgumentNullException(nameof(taskAlarmManagerResolver));
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            if (config == null) throw new ArgumentNullException(nameof(config));
            this.StaffManager = staffManager;
            this.TaskManager = taskManager;
            this.TaskAlarmManagerResolver = taskAlarmManagerResolver;
            this.IMService = imService;
            NotificationManager = notification;
            Config = config; 
        }

        private IStaffManager StaffManager { get; } 

        private ITaskManager TaskManager { get; }

        private IIMService IMService { get; }

        private ILazyResolver<ITaskAlarmManager> TaskAlarmManagerResolver;

        private ITaskAlarmManager TaskAlarmManager {
            get { return TaskAlarmManagerResolver.Required; }
        }

        private INotificationManager NotificationManager { get; }
        
        private IConfiguration Config { get; }

        public  PartakerEntity CreatePartaker(Guid taskId, Guid staffId, PartakerKinds kind,bool isSendMessage=true)
        {
            return InternalCreatePartaker(taskId, staffId, kind,isSendMessage);
        }

        public PartakerEntity CreateCollabrator(Guid taskId, Guid staffId)
        {
            return InternalCreatePartaker(taskId, staffId, PartakerKinds.Collaborator);
        }

        public PartakerEntity CreateMentor(Guid taskId, Guid staffId)
        {
            return InternalCreatePartaker(taskId, staffId, PartakerKinds.Mentor);
        }

        public PartakerEntity CreateRecipient(Guid taskId, Guid staffId)
        {
            return InternalCreatePartaker(taskId, staffId, PartakerKinds.Recipient);
        }

        public PartakerEntity RemoveCollabrator(Guid taskId, Guid staffId)
        {
            return InternalRemovePartaker(taskId, staffId, PartakerKinds.Collaborator);
        }

        public PartakerEntity RemoveMentor(Guid taskId, Guid staffId)
        {
            return InternalRemovePartaker(taskId, staffId, PartakerKinds.Mentor);
        }

        public PartakerEntity RemoveRecipient(Guid taskId, Guid staffId)
        {
            return InternalRemovePartaker(taskId, staffId, PartakerKinds.Recipient);
        }

        public PartakerEntity ChangeLeader(Guid taskId, Guid staffId)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task; 

            //移除原负责人改为协同者
            var oldLeader = task.Partakers.SingleOrDefault(x => x.Kind == PartakerKinds.Leader);
            if (oldLeader != null)
            {
                if (oldLeader.Staff.Id == staffId)
                {
                    return oldLeader;
                }
                oldLeader.Kind = PartakerKinds.Collaborator;
                InternalUpdate(oldLeader);
            }

            //若新接收者已经有其他角色，则移除原有的角色
            PartakerEntity newLeader = FindPartaker(taskId, staffId);
            if (newLeader != null)
            {
                newLeader.Kind = PartakerKinds.Leader;
                InternalUpdate(newLeader);
            }
            else
            {
                newLeader = InternalCreatePartaker(taskId, staffId, PartakerKinds.Leader);
            }
              

            return newLeader;
        } 
         
        public PartakerEntity ExitTask(Guid taskId, Guid staffId)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task;

            PartakerEntity partaker = PartakerExistsResult.CheckForStaff(task, staffId).ThrowIfFailed().Partaker;
             
            InternalRemovePartaker(taskId, staffId,null);
             
            return partaker;
        }

        private PartakerEntity InternalCreatePartaker(Guid taskId, Guid staffId, PartakerKinds partakerKinds,
            bool isSendMessage = true)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.StaffManager, staffId).ThrowIfFailed().Staff;
            PartakerNotExistsResult.CheckStaff(task, staffId).ThrowIfFailed();

            var partaker = new PartakerEntity();
            partaker.Id = Guid.NewGuid();
            partaker.Task = task;
            partaker.Staff = staff;
            partaker.Kind = partakerKinds;
            partaker.CreatedAt = DateTime.Now;

            this.InternalInsert(partaker);


            if (!string.IsNullOrEmpty(task.ConversationId))
            {
                //加入任务的群组
                var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
                IMService.AddMemberAsync(leader.Staff.Id.ToString(), task.ConversationId, staffId.ToString()).Wait();
            }

            var taskAlarms = TaskAlarmManager.FetchAlarmsByPartakerKind(partaker.Task.Id, partaker.Kind).ToList();

            //加入讨论组 
            if (taskAlarms.Any())
            {
                taskAlarms.ForEach(f =>
                {
                    IMService.AddMemberAsync(f.Staff.Id.ToString(), f.ConversationId, staffId.ToString()).Wait();
                });
            }


            if (isSendMessage)
            { 
                var notificationMessage = string.Format(Config["PushMessage:PartakerInv:inv"], staff.Name, task.Name);

                var extra = new Dictionary<string, string>();
                extra.Add("PathTo", "index");
                extra.Add("OrgId", staff.Org.Id.ToString());

                NotificationManager.SendByAliasAsync(null, notificationMessage, extra, staff.Account.PhoneNumber);

                var message = string.Format(Config["LeanCloud:Messages:Task:Join"], staff.Name);
                IMService.SendTextMessageByConversationAsync(task.Id,staff.Account.Id, task.ConversationId, task.Name, message);


            }
            return partaker;
        }

        private PartakerEntity InternalRemovePartaker(Guid taskId, Guid staffId, PartakerKinds? expectedKind)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task; 

            PartakerEntity partaker = PartakerExistsResult.CheckForStaff(task, staffId).ThrowIfFailed().Partaker;
            if (partaker==null || (expectedKind!=null && partaker.Kind != expectedKind))
            {
                throw new FineWorkException($"Staff [{staffId}]is not a {expectedKind} for task [{taskId}].");
            } 
         
            if (partaker.Kind == PartakerKinds.Leader)
            {
                throw new FineWorkException($"管理员必须先移交任务才可以退出.");
            }


            //判断是否有预警或共识为处理 
            PartakerKindUpdateResult.Check(partaker,taskId,this.TaskAlarmManager).ThrowIfFailed(); 

            this.InternalDelete(partaker);

            //移出任务的群组 
            IMService.RemoveMemberAsync(staffId.ToString(), task.ConversationId, staffId.ToString());
            IMService.RemoveConversationAsync(staffId.ToString(), taskId.ToString()).Wait();  
            return partaker;
        }

        public PartakerEntity FindPartaker(Guid partakerId)
        {
            return this.InternalFind(partakerId);
        }

        public PartakerEntity FindPartaker(Guid taskId, Guid staffId)
        {
            return this.InternalFetch(x => x.Task.Id == taskId && x.Staff.Id == staffId).SingleOrDefault();
        }

        public IEnumerable<PartakerEntity> FetchPartakersByTask(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId);
        }

        public IEnumerable<PartakerEntity> FetchPartakersByStaff(Guid staffId)
        {
            return this.InternalFetch(q => q.Where(x => x.Staff.Id == staffId)
                .Include(x => x.Staff)
                .Include(x => x.Task.Partakers.Select(p=>p.Staff.Account))
                .Include(x=>x.Task.Creator.Account));
        }

        public PartakerEntity ChangePartakerKind(TaskEntity task, StaffEntity staff, PartakerKinds partakerKind)
        {
            Args.NotNull(task, nameof(task));
            Args.NotNull(staff, nameof(staff));
            if (partakerKind == PartakerKinds.Leader)
                throw new FineWorkException("不能将用户的角色调整为负责人。");

            var partaker = PartakerExistsResult.CheckForStaff(task, staff.Id).ThrowIfFailed().Partaker;
            if (partaker.Kind == partakerKind)
                throw new FineWorkException("调整的角色不应该跟原角色相同。");

            partaker.Kind = partakerKind;
            InternalUpdate(partaker); 

            return partaker; 
        } 
    }
}
