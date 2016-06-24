using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
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
        public TaskAnnouncementController(ISessionScopeFactory sessionScopeFactory,
            ITaskAnnouncementManager taskAnnouncementManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (taskAnnouncementManager == null) throw new ArgumentNullException(nameof(taskAnnouncementManager)); 
            m_TaskAnnouncementManager = taskAnnouncementManager;
        }
         
        private readonly ITaskAnnouncementManager m_TaskAnnouncementManager;

        [HttpPost("CreateTaskAnnouncement")]
        [DataScoped(true)]
        public TaskAnnouncementViewModel CreateTaskAnnouncement([FromBody]CreateTaskAnnouncementModel announcementModel)
        {
            if (announcementModel == null) throw new ArgumentNullException(nameof(announcementModel));

            var announcement = m_TaskAnnouncementManager.CreateTaskAnnouncement(announcementModel);
            return announcement.ToViewModel();
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

        [HttpPost("ChangeGoodNewsStatus")]
        public void ChangeGoodNewsStatus(Guid announcementId, bool isGoodNews)
        {
            this.m_TaskAnnouncementManager.ChangeGoodNewsStatus(announcementId, isGoodNews);
        }
    }
}
