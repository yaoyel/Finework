using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos.Ambients;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/PartakerReqs")]
    [Authorize("Bearer")]
    public class PartakerReqController : FwApiController
    {
        public PartakerReqController(ISessionScopeFactory sessionScopeFactory,
            ITaskManager taskManager,
            IStaffManager staffManager,
            IPartakerReqManager partakerReqManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (partakerReqManager == null) throw new ArgumentNullException(nameof(partakerReqManager));

            m_SessionScopeFactory = sessionScopeFactory;
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_PartakerReqManager = partakerReqManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;

        private readonly ITaskManager m_TaskManager;

        private readonly IStaffManager m_StaffManager;

        private readonly IPartakerReqManager m_PartakerReqManager;

        /// <summary> 申请加入任务. </summary>
        /// <param name="taskId"> 任务的 <see cref="TaskEntity.Id"/>. </param>
        /// <param name="requestorStaffId"> 申请人的 <see cref="StaffEntity.Id"/>. </param>
        /// <param name="kind"> 申请成为的角色. </param>
        [HttpPost("CreatePartakerReq")]
        [DataScoped(true)]
        public PartakerReqViewModel CreatePartakerReq(Guid taskId, Guid requestorStaffId, PartakerKinds kind)
        {

            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(m_StaffManager, requestorStaffId).ThrowIfFailed().Staff;

            //申请人应当为当前用户
            if (staff.Account.Id != this.AccountId)
            {
                throw new FineWorkException("员工不属于当前登录用户.");
            }
            //任务与员工应当属于同一组织
            if (task.Creator.Org.Id != staff.Org.Id)
            {
                throw new FineWorkException("任务与员工分属不同的组织.");
            }
            //申请人不能已经是任务的参与者
            PartakerNotExistsResult.CheckStaff(task, requestorStaffId).ThrowIfFailed();
            //任务必须开放申请的权限
            PartakerReqIsEnabledResult.Check(task, kind).ThrowIfFailed();

            var req = m_PartakerReqManager.CreatePartakerReq(task, staff, kind);

            return req.ToViewModel();
        }

        /// <summary> 审批任务的加入申请. </summary>
        /// <param name="partakerReqId"> 申请的 <see cref="PartakerReqEntity.Id"/> </param>
        /// <param name="reviewStatus"> 审批的结果. </param>
        [HttpPost("ReviewPartakerReq")]
        [DataScoped(true)]
        public PartakerReqViewModel ReviewPartakerReq(Guid partakerReqId, ReviewStatuses reviewStatus)
        {

            var req = PartakerReqExistsResult.Check(m_PartakerReqManager, partakerReqId).ThrowIfFailed().PartakerReq;
            if (req.ReviewStatus != ReviewStatuses.Unspecified)
            {
                throw new FineWorkException($"Invalid ReviewStatus {reviewStatus}.");
            }
            //申请必须尚未审批
            if (req.ReviewStatus != ReviewStatuses.Unspecified)
            {
                throw new FineWorkException($"申请已经由他人审批.");
            }
            //审批人必须是任务的参与者
            var reviewerPartaker = AccountIsPartakerResult.Check(req.Task, this.AccountId).ThrowIfFailed().Partaker;
            //本方法只能用于审批协同者或指导者
            bool isPartakerKindIsReviewable =
                (req.PartakerKind == PartakerKinds.Collaborator)
                || (req.PartakerKind == PartakerKinds.Mentor);
            if (!isPartakerKindIsReviewable)
            {
                throw new FineWorkException("本方法只能用于审批协同者或指导者.");
            }
            //审批人必须有权审批（审批人必须是任务负责人，或者任务允许成员邀请相应的参与者）
            bool hasReviewAuthority =
                (reviewerPartaker.Kind == PartakerKinds.Leader)
                || (req.PartakerKind == PartakerKinds.Collaborator && req.Task.IsCollabratorInvEnabled)
                || (req.PartakerKind == PartakerKinds.Mentor && req.Task.IsMentorInvEnabled);
            if (!hasReviewAuthority)
            {
                throw new FineWorkException("用户无权审批申请.");
            }

            m_PartakerReqManager.ReviewPartakerReq(req, reviewStatus);

            return req.ToViewModel();

        }
    }
}
