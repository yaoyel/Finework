using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/taskannouncements")]
    [Authorize("Bearer")]
    public class TaskAnnouncementController : FwApiController
    {
        public TaskAnnouncementController(ISessionProvider<AefSession> sessionProvider,
            ITaskAnnouncementManager taskAnnouncementManager)
            : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentNullException(nameof(sessionProvider));
            if (taskAnnouncementManager == null) throw new ArgumentNullException(nameof(taskAnnouncementManager)); 
            m_TaskAnnouncementManager = taskAnnouncementManager;
        }
         
        private readonly ITaskAnnouncementManager m_TaskAnnouncementManager;

        [HttpPost("CreateTaskAnnouncement")]
        //[DataScoped(true)]
        public TaskAnnouncementViewModel CreateTaskAnnouncement([FromBody]CreateTaskAnnouncementModel announcementModel)
        {
            using (var tx = TxManager.Acquire())
            {
                if (announcementModel == null) throw new ArgumentNullException(nameof(announcementModel));

                var announcement = m_TaskAnnouncementManager.CreateTaskAnnouncement(announcementModel);
                var result = announcement.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FetchTaskAnnouncementsByTaskId")]
        public IEnumerable<TaskAnnouncementViewModel> FetchTaskAnnouncementsByTaskId(Guid taskId,
            AnnouncementKinds announcementKind)
        {
            return m_TaskAnnouncementManager.FetchTaskAnnouncementByTaskId(taskId, null, announcementKind)
                .Select(p => p.ToViewModel()).ToList();
        }

        [HttpGet("FetchGoodNewsByStaffId")]
        public IEnumerable<TaskAnnouncementViewModel> FetchGoodNewsByStaffId(Guid staffId)
        {
            return m_TaskAnnouncementManager.FetchTaskAnnouncementByStaffId(staffId, true)
                .Select(p => p.ToViewModel()).ToList();
        }
    }
}
