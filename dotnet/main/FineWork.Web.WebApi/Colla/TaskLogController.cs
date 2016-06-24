using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/tasklogs")]
    [Authorize("Bearer")]
    public class TaskLogController : FwApiController
    {
        public TaskLogController(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            ITaskLogManager taskLogManager
            )
            : base(sessionProvider)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (taskLogManager == null) throw new ArgumentNullException(nameof(taskLogManager));
            m_TaskManager = taskManager;
            m_TaskLogManager = taskLogManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly ITaskLogManager m_TaskLogManager;

        [HttpGet("FetchTaskLogByTaskId")]
        public IEnumerable<TaskLogViewModel> FetchTaskLogByTaskId(Guid taskId)
        {
            return m_TaskLogManager.FetchTaskLogByTaskId(taskId)
                .Select(p => p.ToViewModel());
        }

        [HttpGet("FetchUpdateLogByTaskId")]
        public IActionResult FetchUpdateLogByTaskId(Guid taskId,string columnName, bool isLastOneOnly = true)
        {
            var result = m_TaskLogManager.FetchUpdateLogByTaskId(taskId,columnName).ToList();
            if (!result.Any())
                return new HttpNotFoundObjectResult(taskId);
            if (isLastOneOnly)
                return new ObjectResult(result.FirstOrDefault().ToViewModel());
            return new ObjectResult(result.Select(p => p.ToViewModel()));

        }

        [HttpGet("FetchExcitationLogByTaskId")]
        public IEnumerable<TaskLogViewModel> FetchExcitationLogByTaskId(Guid taskId)
        {
            //TODO 激励的kind
            return m_TaskLogManager.FetchExcitationLogByTaskId(taskId)
                .Select(p => p.ToViewModel());
        }

    }
}
