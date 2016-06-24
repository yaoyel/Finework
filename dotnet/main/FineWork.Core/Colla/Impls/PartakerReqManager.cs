using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Avatar;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Core;
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class PartakerReqManager : AefEntityManager<PartakerReqEntity, Guid>, IPartakerReqManager
    {
        public PartakerReqManager(ISessionProvider<AefSession> dbContextProvider,
            ILazyResolver<IPartakerInvManager> invManagerResolver,
            IPartakerManager partakerManager,
            IIMService imService
            )
            : base(dbContextProvider)
        {
            this.InvManagerResolver = invManagerResolver;
            PartakerManager = partakerManager;
            IMService = imService;
        }

        internal IPartakerManager PartakerManager { get; set; }
        internal ILazyResolver<IPartakerInvManager> InvManagerResolver { get; private set; } 
        internal IPartakerInvManager InvManager
        {
            get { return this.InvManagerResolver.Required; }
        }

        private IIMService IMService { get; }

        public PartakerReqEntity CreatePartakerReq(TaskEntity task, StaffEntity requestorStaff, PartakerKinds kind)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (requestorStaff == null) throw new ArgumentNullException(nameof(requestorStaff));

            //任务必须开放相应 kind 的申请
            PartakerReqIsEnabledResult.Check(task, kind).ThrowIfFailed();

            //不应存在相同 kind 的邀请，客户端在创建申请前，应先检查有无未处理的邀请
            //PartakerInvNotExistsResult.Check(this.InvManager, task, requestorStaff, kind).ThrowIfFailed();
            PartakerReqIsEnabledResult.Check(task, kind).ThrowIfFailed();

            PartakerReqEntity result = null;
            //是否已经存在申请
            result = PartakerReqExistsResult.CheckPending(this, task, requestorStaff).PartakerReq;
            if (result != null)
            {
                result.CreatedAt = DateTime.Now;
                result.PartakerKind = kind;
                this.InternalUpdate(result);
                return result;
            }

            result = new PartakerReqEntity();
            result.Id = Guid.NewGuid();
            result.Task = task;
            result.Staff = requestorStaff;
            result.PartakerKind = kind;
            result.ReviewStatus = ReviewStatuses.Unspecified;
            result.CreatedAt = DateTime.Now;

            this.InternalInsert(result);

            return result;
        }

        public PartakerReqEntity ReviewPartakerReq(PartakerReqEntity partakerReq, ReviewStatuses reviewStatus)
        {
            if (partakerReq == null) throw new ArgumentNullException(nameof(partakerReq));
            if (partakerReq.ReviewStatus != ReviewStatuses.Unspecified)
            {
                throw new FineWorkException($"Invalid ReviewStatus {reviewStatus}.");
            }

            PartakerNotExistsResult.CheckStaff(partakerReq.Task, partakerReq.Staff.Id).ThrowIfFailed();

            partakerReq.ReviewStatus = reviewStatus;
            partakerReq.ReviewAt = DateTime.Now; 

            this.InternalUpdate(partakerReq);

            if (reviewStatus == ReviewStatuses.Approved)
            {
                //加入task_partakers  
                PartakerNotExistsResult.CheckStaff(partakerReq.Task, partakerReq.Staff.Id).ThrowIfFailed();

                //加入任务对应的群组
                var leader = partakerReq.Task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
                IMService.AddMemberAsync(leader.Staff.Id.ToString(), partakerReq.Task.ConversationId,
                    partakerReq.Staff.Id.ToString());

                this.PartakerManager.CreatePartaker(partakerReq.Task.Id, partakerReq.Staff.Id, partakerReq.PartakerKind);
            }
            return partakerReq;
        }

        public PartakerReqEntity FindPartakerReq(Guid partakerReqId)
        {
            return this.InternalFind(partakerReqId);
        }

        public IEnumerable<PartakerReqEntity> FetchPartakerReqsByTask(Guid taskId)
        {
            return this.InternalFetch(x => x.Task.Id == taskId);
        }

        public IEnumerable<PartakerReqEntity> FetchPartakerReqsByStaff(Guid staffId)
        {
            return this.InternalFetch(x => x.Staff.Id == staffId);
        }

        public IEnumerable<PartakerReqEntity> FetchPendingPartakerReqs(Guid taskId, Guid staffId)
        {
            return this.InternalFetch(
                x => x.Task.Id == taskId && x.Staff.Id == staffId
                     && x.ReviewStatus == ReviewStatuses.Unspecified);
        }

        public IEnumerable<PartakerReqEntity> FetchPendingPartakerReqsByTask(Guid taskId)
        {
            //获取已经加入的成员
            var partakerIds = this.PartakerManager.FetchPartakersByTask(taskId).Select(p => p.Staff.Id).ToArray();
            

            return this.InternalFetch(x => x.Task.Id == taskId && x.ReviewStatus == ReviewStatuses.Unspecified
             && !partakerIds.Contains(x.Staff.Id));
        }

        public IEnumerable<PartakerReqEntity> FetchPendingPartakerReqsByStaff(Guid staffId)
        { 
            return this.InternalFetch(x => x.Staff.Id == staffId && x.ReviewStatus == ReviewStatuses.Unspecified);
        }
    }
}