using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/tasklogs")]
    public class TaskLogController : FwApiController
    {
        public TaskLogController(ISessionScopeFactory sessionScopeFactory,
            ITaskManager taskManager,
            ITaskLogManager taskLogManager
            )
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (taskLogManager == null) throw new ArgumentNullException(nameof(taskLogManager));
            m_SessionScopeFactory = sessionScopeFactory;
            m_TaskManager = taskManager;
            m_TaskLogManager = taskLogManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;
        private readonly ITaskManager m_TaskManager;
        private readonly ITaskLogManager m_TaskLogManager;

        [HttpGet("FetchTaskLogByTaskId")]
        public IEnumerable<TaskLogViewModel> FetchTaskLogByTaskId(Guid taskId)
        {
            return m_TaskLogManager.FetchTaskLogByTaskId(taskId)
                .Select(p => p.ToViewModel());
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
