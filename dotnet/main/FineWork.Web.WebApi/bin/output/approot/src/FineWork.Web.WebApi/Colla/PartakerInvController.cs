using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos.Ambients;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/PartakerInvs")]
    [Authorize("Bearer")]
    public class PartakerInvController : FwApiController
    {
        public PartakerInvController(ISessionScopeFactory sessionScopeFactory,
            ITaskManager taskManager,
            IStaffManager staffManager,
            IPartakerInvManager partakerInvManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager));

            m_SessionScopeFactory = sessionScopeFactory;
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_PartakerInvManager = partakerInvManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;

        private readonly ITaskManager m_TaskManager;

        private readonly IStaffManager m_StaffManager;

        private readonly IPartakerInvManager m_PartakerInvManager;

        /// <summary> 直接将员工加入任务. </summary>
        /// <param name="taskId"> 任务的 <see cref="TaskEntity.Id"/>. </param>
        /// <param name="inviteeStaffId"> 被加入的员工的 <see cref="StaffEntity.Id"/>. </param>
        /// <param name="partakerKind"> 担任的角色. </param>
        [HttpPost("QuickAdd")]
        [DataScoped(true)]
        public PartakerInvViewModel QuickAdd(Guid taskId, Guid inviteeStaffId, PartakerKinds partakerKind)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var inviteeStaff = StaffExistsResult.Check(m_StaffManager, inviteeStaffId).ThrowIfFailed().Staff;
            var inviterPartaker =
                AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            PartakerInvIsEnabledResult.Check(task, inviterPartaker, partakerKind).ThrowIfFailed();

            var inviterStaff = inviterPartaker.Staff;
            var inv = m_PartakerInvManager.QuickAdd(task, inviterStaff, inviteeStaff, partakerKind);

            return inv.ToViewModel();
        } 

        [HttpPost("CreatePartakerInv")]
        [DataScoped(true)]
        public PartakerInvViewModel CreatePartakerInv(Guid taskId, Guid inviteeStaffId, PartakerKinds partakerKind)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var inviteeStaff = StaffExistsResult.Check(m_StaffManager, inviteeStaffId).ThrowIfFailed().Staff;
            var inviterPartaker =
                AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            PartakerInvIsEnabledResult.Check(task, inviterPartaker, partakerKind).ThrowIfFailed();

            var inviterStaff = inviterPartaker.Staff;
            var inv = m_PartakerInvManager.CreatePartakerInv(task, inviterStaff, inviteeStaff, partakerKind);

            return inv.ToViewModel();
        }

        [HttpPost("ReviewPartakerInv")]
        public IActionResult ReviewPartakerInv(Guid partakerInvId, ReviewStatuses newRevStatus)
        {
            var partakerInv =
                PartakerInvExistsResult.CheckPending(this.m_PartakerInvManager, partakerInvId)
                    .ThrowIfFailed()
                    .PartakerInv;
            this.m_PartakerInvManager.ReviewPartakerInv(partakerInv, newRevStatus);

            return new HttpStatusCodeResult(200);
        }

        [HttpGet("FetchPartakerInvsByStaff")]
        public IActionResult FetchPartakerInvsByStaff(Guid staffId)
        {
            var partakerInvs = this.m_PartakerInvManager.FetchPendingPartakerInvsByStaff(staffId).ToList();
            if (partakerInvs.Any())
                return new ObjectResult( partakerInvs.Select(p => p.ToViewModel()));
            return new HttpNotFoundObjectResult(staffId);
        } 
    }
}
