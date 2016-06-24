using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class PartakerInvManager : AefEntityManager<PartakerInvEntity, Guid>, IPartakerInvManager
    {
        public PartakerInvManager(ISessionProvider<AefSession> dbContextProvider, 
            ILazyResolver<IPartakerManager> partakerManagerResolver, 
            ILazyResolver<IPartakerReqManager> reqManagerResolver,
            IIMService imService,
            INotificationManager notificationManager,
            IConfiguration config)
            : base(dbContextProvider)
        {
            if (partakerManagerResolver == null) throw new ArgumentNullException(nameof(partakerManagerResolver));
            if (reqManagerResolver == null) throw new ArgumentNullException(nameof(reqManagerResolver));
            if (imService == null) throw new ArgumentNullException(nameof(imService));
            if (notificationManager == null) throw new ArgumentNullException(nameof(notificationManager));

            this.PartakerManagerResolver = partakerManagerResolver;
            this.ReqManagerResolver = reqManagerResolver;
            IMService = imService;
            NotificationManager = notificationManager;
            Config = config;
        }

        internal ILazyResolver<IPartakerManager> PartakerManagerResolver { get; private set; }

        private IIMService IMService { get; }

        private IConfiguration Config { get; }

        private INotificationManager NotificationManager { get; }

        internal IPartakerManager PartakerManager
        {
            get { return this.PartakerManagerResolver.Required; }
        }

        internal ILazyResolver<IPartakerReqManager> ReqManagerResolver { get; private set; }

        internal IPartakerReqManager ReqManager
        {
            get { return this.ReqManagerResolver.Required; }
        } 

        public PartakerInvEntity QuickAdd(TaskEntity task, StaffEntity inviterStaff, StaffEntity inviteeStaff,
            PartakerKinds partakerKind)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (inviterStaff == null) throw new ArgumentNullException(nameof(inviterStaff));
            if (inviteeStaff == null) throw new ArgumentNullException(nameof(inviteeStaff)); 

            PartakerEntity partakerTaskLeader = null;

            if (task.ParentTask != null && task.ParentTask.Partakers.Any())
                partakerTaskLeader = task.ParentTask.Partakers.FirstOrDefault(p => p.Kind == PartakerKinds.Leader);

            if (partakerTaskLeader != null && partakerTaskLeader.Staff != inviterStaff)
            {
                var inviterPartaker = PartakerExistsResult.CheckForStaff(task, inviterStaff.Id).ThrowIfFailed().Partaker;

                PartakerInvIsEnabledResult.Check(task, inviterPartaker, partakerKind).ThrowIfFailed();

                //受邀人不能是任务成员
                if (PartakerNotExistsResult.CheckStaff(task, inviteeStaff.Id).Partaker != null)
                {
                    throw new FineWorkException($"[{inviteeStaff.Name}] 已加入该任务，不允许重复邀请！");
                }

                //邀请人必须是任务成员
                var inviterKind = task.PartakerKindFor(inviterStaff);
                if (inviterKind == PartakerKinds.Unspecified)
                {
                    throw new FineWorkException($"[{inviterStaff.Name}] 无权为任务 [{task.Name}] 邀请成员.");
                }
            }

            PartakerInvEntity result = new PartakerInvEntity();
            //已经有人邀请，修改inviterNames
            var partakerInv = PartakerInvExistsResult.CheckPending(this, task, inviteeStaff, partakerKind).PartakerInv;
            if (partakerInv != null && partakerInv.PartakerKind == partakerKind)
            {
                if (!partakerInv.InviterStaffIds.Contains(inviterStaff.Id.ToString()))
                    partakerInv.InviterStaffIds = string.Concat(inviterStaff.Id.ToString(), ",",
                        partakerInv.InviterStaffIds);
                partakerInv.CreatedAt = DateTime.Now;
                partakerInv.ReviewStatus = ReviewStatuses.Unspecified;
                this.InternalUpdate(partakerInv);
                result = partakerInv;
            } 
            else
            {
                result.Id = Guid.NewGuid();
                result.Task = task;
                result.Staff = inviteeStaff;
                result.PartakerKind = partakerKind;
                result.Message = $"{inviterStaff.Name} 邀请 {inviteeStaff.Name} 加入任务.";
                result.InviterStaffIds = inviterStaff.Id.ToString();
                result.CreatedAt = DateTime.Now;
                result.ReviewStatus = ReviewStatuses.Unspecified;
                result.ReviewAt = DateTime.Now;
                this.InternalInsert(result);
            }
            switch (partakerKind)
            {
                case PartakerKinds.Collaborator:
                    this.PartakerManager.CreatePartaker(task.Id, inviteeStaff.Id,PartakerKinds.Collaborator,false);
                    break;
                case PartakerKinds.Mentor:
                    this.PartakerManager.CreatePartaker(task.Id, inviteeStaff.Id,PartakerKinds.Mentor,false);
                    break;
                case PartakerKinds.Recipient:
                    this.PartakerManager.CreatePartaker(task.Id, inviteeStaff.Id,PartakerKinds.Recipient,false);
                    break;
                default:
                    throw new FineWorkException("本方法只能用于处理指导者，协同者，接受者的邀请");
            }
             
            return result; 
        } 

        public PartakerInvEntity ReviewPartakerInv(PartakerInvEntity partakerInvEntity, ReviewStatuses newRevStatus)
        {
            Args.NotNull(partakerInvEntity, nameof(partakerInvEntity));

            if(partakerInvEntity.ReviewStatus!=ReviewStatuses.Unspecified)
                throw new FineWorkException($"{partakerInvEntity.Staff.Name}已经{partakerInvEntity.ReviewStatus.GetLabel()}加入该任务。");

            PartakerNotExistsResult.CheckStaff(partakerInvEntity.Task, partakerInvEntity.Staff.Id).ThrowIfFailed();

            if (newRevStatus == ReviewStatuses.Approved)
            {
                switch (partakerInvEntity.PartakerKind)
                {
                    case PartakerKinds.Collaborator:
                        this.PartakerManager.CreateCollabrator(partakerInvEntity.Task.Id, partakerInvEntity.Staff.Id);
                        break;
                    case PartakerKinds.Mentor:
                        this.PartakerManager.CreateMentor(partakerInvEntity.Task.Id, partakerInvEntity.Staff.Id);
                        break;
                    case PartakerKinds.Recipient:
                        this.PartakerManager.CreateRecipient(partakerInvEntity.Task.Id,partakerInvEntity.Staff.Id);
                        break;
                    default:
                        throw new FineWorkException("本方法只能用于处理指导者，协同者，接受者的邀请");
                }

                //加入群组聊天室
                var leader = partakerInvEntity.Task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
                IMService.AddMemberAsync(leader.Staff.Id.ToString(), partakerInvEntity.Task.ConversationId,
                    partakerInvEntity.Staff.Id.ToString());
            }
            partakerInvEntity.ReviewStatus = newRevStatus;
            partakerInvEntity.ReviewAt = DateTime.Now;
            InternalUpdate(partakerInvEntity); 

            return partakerInvEntity;
        }

        public PartakerInvEntity FindPartakerInv(Guid partakerInvId)
        {
            return InternalFind(partakerInvId);
        }

        public IEnumerable<PartakerInvEntity> FetchPendingPartakerInvs(Guid taskId, Guid staffId)
        {
            return this.InternalFetch(
                x => x.Task.Id == taskId && x.Staff.Id == staffId
                     && x.ReviewStatus == ReviewStatuses.Unspecified);
        }

        public IEnumerable<PartakerInvEntity> FetchPendingPartakerInvsByTask(Guid taskId)
        {
            return this.InternalFetch(
                x => x.Task.Id == taskId && x.ReviewStatus == ReviewStatuses.Unspecified);
        }

        public IEnumerable<PartakerInvEntity> FetchPendingPartakerInvsByStaff(Guid staffId)
        {
            return this.InternalFetch(x => x.Staff.Id == staffId);
        }

        public IEnumerable<PartakerInvEntity> FetchSentPartakerInvsByStaff(Guid staffId)
        {
            return this.InternalFetch(x => x.InviterStaffIds.Contains(staffId.ToString()));
        }
    }
}
