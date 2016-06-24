using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using System.Net;
using System.Text;
using AppBoot.Transactions;
using FineWork.Core.Colla.Models;
using FineWork.Message;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/StaffInvs")]
    [Authorize("Bearer")]
    public class StaffInvController : FwApiController
    {
        public StaffInvController(ISessionScopeFactory sessionScopeFactory,
            IStaffInvManager staffInvManager,
            IOrgManager orgManager,
            INotificationManager notificationManager,
            IApplicationEnvironment appEnv
            )
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentException(nameof(sessionScopeFactory));
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));
            if (staffInvManager == null) throw new ArgumentException(nameof(staffInvManager));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));


            m_SessionScopeFactory = sessionScopeFactory;
            m_StaffInvManager = staffInvManager;
            m_NotificationManager = notificationManager;
            m_OrgManager = orgManager;

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath);

            configBuilder
                .AddJsonFile(@"Configs\pushSettings.json", false);
            Configuration = configBuilder.Build();

        }


        private readonly ISessionScopeFactory m_SessionScopeFactory;
        private readonly IStaffInvManager m_StaffInvManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IOrgManager m_OrgManager;
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invStaffs">item1:name,item2:phonenumber</param>
        [HttpPost("CreateStaffInvs")]
        [DataScoped(true)]
        public IActionResult BulkCreateStuffInv([FromBody] CreateStaffInvModel invStaffs)
        {
            if (invStaffs == null)
                throw new ArgumentException(nameof(invStaffs));

            invStaffs.InviterName = this.AccountName;
            var org = m_OrgManager.FindOrg(invStaffs.OrgId);
            m_StaffInvManager.BulkCreateStuffInv(invStaffs);

            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "staffInv");

            m_NotificationManager.SendByAliasAsync(null,
                string.Format(Configuration.Get("PushMessage:StaffInv:description"), this.AccountName, org.Name),
                extra, invStaffs.Invitees.Select(p => p.Item2).ToArray())
                .Wait();

            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
        }

        [HttpPost("CreateStaffInv")]
        [DataScoped(true)]
        public StaffInvViewModel CreateStaffInv(Guid accountId, Guid orgId)
        {
            var staffInv = m_StaffInvManager.CreateStaffInv(accountId, this.AccountName, orgId);
            return staffInv.ToViewModel();
        }

        [HttpGet("FetchStaffInvsWithStaffsByAccount")]

        public IList<StaffInvViewModel> FetchStaffInvsWithStaffsByAccount()
        {
            return m_StaffInvManager.FetchStaffInvsByAccount(this.AccountId)
                .Select(p => p.ToViewModel()).ToList();
        }

        [HttpPost("UpdateReviewStatus")]
        public IActionResult ChangeReviewStatus(Guid staffInvId, short newStatus)
        {
            m_StaffInvManager.ChangeReviewStatus(m_StaffInvManager.FindStaffInvById(staffInvId),
                (ReviewStatuses) newStatus);

            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);

        }
    }
}
