using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using FineWork.Colla.Checkers;
using AppBoot.Checks;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/TaskTransfers")]
    [Authorize("Bearer")]
    public class TaskTransferController : FwApiController
    {
        public TaskTransferController(ISessionScopeFactory sessionScopeFactory,
            IStaffManager staffManager,
            ITaskManager taskManager,
            ITaskTransferManager taskTransferManager)
            : base(sessionScopeFactory)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(taskTransferManager, nameof(taskTransferManager));

            m_StaffManager = staffManager;
            m_TaskManager = taskManager;
            m_TaskTransferManager = taskTransferManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly ITaskManager m_TaskManager;
        private readonly ITaskTransferManager m_TaskTransferManager;

        [HttpPost("CreateTaskTransfer")]
        [DataScoped(true)]
        public TaskTransferEntity CreateTaskTransfer(Guid staffId, Guid taskId, Guid attachedTaskId)
        {
            return m_TaskTransferManager.CreateTaskTransfer(staffId, taskId, attachedTaskId);
        }

        [HttpPost("ReviewAttTaskTransfer")]
        [DataScoped(true)]
        public void ReviewAttTaskTransfer(Guid taskTransferId, ReviewStatuses attStatus)
        {
            var taskTransfer =
                TaskTransferExistsResult.Check(this.m_TaskTransferManager, taskTransferId).ThrowIfFailed().TaskTransfer;

            //判断当前用户是不是要迁入任务的leader
            AccountIsPartakerResult.Check(taskTransfer.AttachedTask, this.AccountId, PartakerKinds.Leader)
                .ThrowIfFailed();

            m_TaskTransferManager.ReviewAttTaskTransfer(taskTransfer, attStatus);
        }

        [HttpPost("ReviewDetTaskTransfer")]
        [DataScoped(true)]
        public void ReviewDetTaskTransfer(Guid taskTransferId, ReviewStatuses detStatus)
        {
            var taskTransfer =
                TaskTransferExistsResult.Check(this.m_TaskTransferManager, taskTransferId).ThrowIfFailed().TaskTransfer;

            //判断当前用户是不是要迁出任务的leader
            AccountIsPartakerResult.Check(taskTransfer.Task.ParentTask, this.AccountId, PartakerKinds.Leader).ThrowIfFailed();

            m_TaskTransferManager.ReviewAttTaskTransfer(taskTransfer, detStatus);
        }

        [HttpGet("FetchTaskTransfersByTask")]
        public IActionResult FetchTaskTransfersByTask(Guid taskId)
        {
            var taskTransfers = m_TaskTransferManager.FetchTaskTransferByTask(taskId).ToList();

            if (taskTransfers.Any())
            {
                //任务迁出申请
                var dets =
                    taskTransfers.Where(p => p.Task.ParentTask != null && p.Task.ParentTask.Id == taskId).ToList()
                        .Select(p => p.ToViewModel(this.m_StaffManager));

                //任务迁入申请
                var atts = taskTransfers.Where(p => p.AttachedTask.Id == taskId && p.Task.ParentTask != null).ToList()
                    .Select(p => p.ToViewModel(this.m_StaffManager)); 
                //变更上级任务申请
                var transfers = taskTransfers.Where(p => p.Task.Id == taskId).ToList()
                    .Select(p => p.ToViewModel(this.m_StaffManager)); 

                //子任务加入申请
                var joinIns =
                    taskTransfers.Where(p => p.Task.ParentTask == null && p.AttachedTask.Id == taskId).ToList()
                        .Select(p => p.ToViewModel(this.m_StaffManager)); 

                var result = new { Dets=dets,Atts=atts,Transfers = transfers,JoinIns=joinIns};

                return new ObjectResult(result);
            }

            return new HttpNotFoundObjectResult(taskId);
        }


    }
}
