using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Incentives")]
    [Authorize("Bearer")]
    public class IncentiveController : FwApiController
    {
        public IncentiveController(ISessionScopeFactory sessionScopeFactory,
            ITaskIncentiveManager taskIncentiveManager,
            IIncentiveKindManager incentiveKindManager
            ) : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (taskIncentiveManager == null) throw new ArgumentNullException(nameof(taskIncentiveManager));
            if (incentiveKindManager == null) throw new ArgumentNullException(nameof(incentiveKindManager));

            m_TaskIncentiveManager = taskIncentiveManager;
            m_IncentiveKindManager = incentiveKindManager;
        }

        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;

        [HttpGet("FetchTaskIncentivesByTaskId")]
        public IActionResult FetchTaskIncentivesByTaskId(Guid taskId)
        {
            var taskIncentives = this.m_TaskIncentiveManager.FetchTaskIncentiveByTaskId(taskId);
             
            return taskIncentives != null? new ObjectResult(taskIncentives
                .Select(p => p.ToViewModel())
                .OrderBy(p=>p.IncentiveKind.Id))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpPost("UpdateTaskIncentive")]
        [DataScoped(true)]
        public TaskIncentiveViewModel UpdateTaskIncentive(Guid taskId, int incentiveKindId, int amount)
        {
            return this.m_TaskIncentiveManager.UpdateTaskIncentive(taskId, incentiveKindId, amount).ToViewModel();
        }

        [HttpGet("FetchIncentiveKind")]
        public IEnumerable<IncentiveKindViewModel> FetchIncentiveKind()
        {
            return
                this.m_IncentiveKindManager.FetchIncentiveKind()
                    .Select(p => p.ToViewModel());
        }
    }
}
