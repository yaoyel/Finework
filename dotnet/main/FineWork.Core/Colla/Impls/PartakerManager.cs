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
            IConfiguration config,
            IMemberManager memberManager)
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
            m_MemberManager = memberManager;
        }

        private IStaffManager StaffManager { get; } 

        private ITaskManager TaskManager { get; }

        private IIMService IMService { get; }

        private ILazyResolver<ITaskAlarmManager> TaskAlarmManagerResolver;

        private IMemberManager m_MemberManager;
        private ITaskAlarmManager TaskAlarmManager {
            get { return TaskAlarmManagerResolver.Required; }
        }

        private INotificationManager NotificationManager { get; }
        
        private IConfiguration Config { get; }

        public  PartakerEntity CreatePartaker(Guid taskId, Guid staffId, PartakerKinds kind,bool sendMessage=true)
        {
            return InternalCreatePartaker(taskId, staffId, kind,sendMessage);
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

        public PartakerEntity ChangeLeader(Guid taskId, Guid staffId, PartakerKinds newKind)
        {
            var task = TaskExistsResult.Check(this.TaskManager, taskId).ThrowIfFailed().Task;  
       
            var oldLeader = task.Partakers.SingleOrDefault(x => x.Kind == PartakerKinds.Leader);
            if (oldLeader != null)
            {
                if (oldLeader.Staff.Id == staffId)
                {
                    return oldLeader;
                }
                oldLeader.Kind = newKind;
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

        private PartakerEntity InternalCreatePartaker(Guid taskId, Guid staffId, PartakerKinds partakerKinds,bool sendMessage=true)
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


            if (task.Conversation != null)
            {
                //加入任务的群组
                var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
                IMService.AddMemberAsync(leader.Staff.Id.ToString(), task.Conversation.Id, staffId.ToString()).Wait();
                m_MemberManager.CreateMember(task.ConversationId, staff.Id);
            } 

            if (sendMessage && task.Conversation != null)
            {
                var message = string.Format(Config["LeanCloud:Messages:Task:Join"], staff.Name);
                IMService.SendTextMessageByConversationAsync(task.Id, staff.Account.Id, task.Conversation.Id,
                    task.Name, message);

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
            PartakerKindUpdateResult.Check(partaker,taskId,this.TaskAlarmManager,true).ThrowIfFailed(); 

            this.InternalDelete(partaker);

            //移出任务的群组 
            m_MemberManager.DeleteMember(task.ConversationId,staffId);
            IMService.RemoveMemberAsync(staffId.ToString(), task.Conversation.Id, staffId.ToString());
            IMService.RemoveConversationByStaffIdAsync(staffId.ToString(), taskId.ToString()).Wait();  
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
            return this.InternalFetch(q => q.Where(x => x.Staff.Id == staffId && x.Task.IsDeserted==null)
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


        public void UpdatePartaker(PartakerEntity partaker)
        {
            Args.NotNull(partaker, nameof(partaker));
            this.InternalUpdate(partaker);
        }

        public IEnumerable<PartakerEntity> FetchExilsesByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId && p.IsExils.HasValue && p.IsExils.Value);
        }

        public IEnumerable<PartakerEntity> FetchPartakerByKind(Guid taskId, PartakerKinds kind)
        {
            return this.InternalFetch(p => p.Task.Id == taskId && p.Kind == kind);
        }


        public void DeletePartakersByTaskId(Guid taskId)
        {
            var partakers = this.InternalFetch(p => p.Task.Id == taskId).ToList();

            if(partakers.Any())
                partakers.ForEach(InternalDelete);
        }
    }
}
