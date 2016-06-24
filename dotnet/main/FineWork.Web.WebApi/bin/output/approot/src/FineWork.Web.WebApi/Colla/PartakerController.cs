using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using  AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Partakers")]
    [Authorize("Bearer")]
    public class PartakerController : FwApiController
    {
        public PartakerController(ISessionScopeFactory sessionScopeFactory,
            ITaskManager taskManager, 
            IStaffManager staffManager,
            IPartakerManager partakerManager,
            IPartakerReqManager partakerReqManager)
            : base(sessionScopeFactory)
        { 
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));

            m_PartakerManager = partakerManager;
            m_PartakerReqManager = partakerReqManager;
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
        }
         
        private readonly IPartakerManager m_PartakerManager;
        private readonly IPartakerReqManager m_PartakerReqManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;

        [HttpGet("FetchPartakersWithReqsByStaff")]
        public IActionResult FetchPartakersWithReqsByStaff(Guid staffId)
        {
            Func<TaskViewModel, IEnumerable<PartakerDetailViewModel>> fetchPartakers =
                (task) => m_PartakerManager.FetchPartakersByTask(task.Id)
                    .Select(p => p.ToDetailViewModel());

            Func<TaskViewModel, IEnumerable<PartakerReqViewModel>> fetchPartakerReqs =
                (task) => m_PartakerReqManager.FetchPendingPartakerReqsByTask(task.Id)
                .Select(p => p.ToViewModel());

            var partakers = m_PartakerManager.FetchPartakersByStaff(staffId).ToList();
            if (!partakers.Any())
                return new HttpNotFoundObjectResult(staffId);

            var result = partakers.Select(p => p.ToDetailViewModel()).
            GroupBy(p => p.Task)
            .Select(p => new
            {
                Task = p.Key,
                Partakers = fetchPartakers(p.Key),
                PartakerReqs = fetchPartakerReqs(p.Key)
            }).ToList();
            return new ObjectResult(result);
        }


        [HttpGet("FetchPartakersByStaff")]
        public IActionResult FetchPartakersByStaff(Guid staffId)
        {
            var partakers = m_PartakerManager.FetchPartakersByStaff(staffId);
            return partakers == null
                ? new HttpNotFoundObjectResult(staffId)
                : new ObjectResult(partakers.Select(p => p.ToViewModel()).ToList());
        }

        /// <summary>
        /// 创建协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("CreateCollabrator")]
        [DataScoped(true)]
        public PartakerViewModel CreateCollabrator(Guid taskId, Guid staffId)
        {
            var collabrator = this.m_PartakerManager.CreateCollabrator(taskId, staffId);
            return collabrator.ToViewModel();
        }

        /// <summary>
        /// 创建指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("CreateMentor")]
        [DataScoped(true)]
        public PartakerViewModel CreateMentor(Guid taskId, Guid staffId)
        {
            var collabrator = this.m_PartakerManager.CreateMentor(taskId, staffId);
            return collabrator.ToViewModel();
        }

        /// <summary>
        /// 移除协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemoveCollabrator")]
        [DataScoped(true)]
        public IActionResult RemoveCollabrator(Guid taskId, Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            if (staff.Account.Id == this.AccountId)
                throw new FineWorkException("用户无法删除自己的角色");

             this.m_PartakerManager.RemoveCollabrator(taskId, staffId);

            return new HttpStatusCodeResult((int)HttpStatusCode.NoContent);
        }

        /// <summary>
        /// 移除指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemoveMentor")]
        [DataScoped(true)]
        public PartakerViewModel RemoveMentor(Guid taskId, Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            if (staff.Account.Id == this.AccountId)
                throw new FineWorkException("用户无法删除自己的角色");

            var collabrator = this.m_PartakerManager.RemoveMentor(taskId, staffId);
            return collabrator.ToViewModel();
        }

        /// <summary>
        /// 移交任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("ChangeLeader")]
        [DataScoped(true)]
        public PartakerViewModel ChangeLeader(Guid taskId, Guid staffId)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            if (partaker.Kind == PartakerKinds.Collaborator)
                throw new FineWorkException("任务协作者不可用进行此操作。");

            var leader = this.m_PartakerManager.ChangeLeader(taskId, staffId);
            return leader.ToViewModel();
        }

        /// <summary>
        /// 退出任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemovePartaker")]
        public PartakerViewModel RemovePartaker(Guid taskId, Guid staffId)
        {
            var partaker = this.m_PartakerManager.RemovePartaker(taskId, staffId);
            return partaker.ToViewModel();
        }

        /// <summary>
        /// 更新接受者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>

        [HttpPost("ChangeRecipient")]
        [DataScoped(true)]
        public PartakerViewModel ChangeRecipient(Guid taskId, Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            if (staff.Account.Id == this.AccountId)
                throw new FineWorkException("用户无法更新自己的角色");

            var recipient = this.m_PartakerManager.ChangeRecipient(taskId, staffId);
            return recipient.ToViewModel();
        }

        /// <summary>
        ///  获取战友信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpGet("FetchPartakersByOrg")]
        public IActionResult FetchPartakersByOrg(Guid orgId, Guid staffId)
        {
            //获取组织下所有与我相关的任务
            var tasks=m_TaskManager.FetchTasksByOrgId(orgId).ToList();
            var partakers = new List<PartakerEntity>();
            if (tasks.Any())
            {
                  partakers = tasks.Where(p => p.Partakers.Any(s => s.Staff.Id == staffId))
                    .SelectMany(p=>p.Partakers).ToList(); 
            }
            var result = partakers.Any()
                 ? new ObjectResult(partakers.Select(p => p.ToViewModel()))
                 : new HttpNotFoundObjectResult($"orgid:{orgId},staffId:{staffId}");
            return result;
        }

        [HttpPost("ChangePartakerKind")]
        public PartakerViewModel ChangePartakerKind(Guid taskId, Guid staffId, PartakerKinds partakerKind)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var partaker=AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
             
            //协同者不可以使用调整角色功能
            if (partaker.Kind == PartakerKinds.Collaborator)
                throw new FineWorkException("协同者不可以使用调整角色功能");
            var result=this.m_PartakerManager.ChangePartakerKind(task, staff, partakerKind);
            return result.ToViewModel(); 
        }
    }
}
