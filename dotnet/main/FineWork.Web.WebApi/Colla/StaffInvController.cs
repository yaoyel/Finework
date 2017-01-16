using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using System.Net;
using System.Text;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
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
        public StaffInvController(ISessionProvider<AefSession> sessionProvider,
            IStaffInvManager staffInvManager,
            IOrgManager orgManager,
            INotificationManager notificationManager,IConfiguration config)
            : base(sessionProvider)
        {
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));
            if (staffInvManager == null) throw new ArgumentException(nameof(staffInvManager));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));
            if (config == null) throw new ArgumentException(nameof(config));

            m_StaffInvManager = staffInvManager;
            m_OrgManager = orgManager;
            m_Configuration = config;
            m_NotificationManager = notificationManager;
        }


        private readonly IStaffInvManager m_StaffInvManager;
        private readonly IOrgManager m_OrgManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IConfiguration m_Configuration;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invStaffs">item1:name,item2:phonenumber</param>
        [HttpPost("CreateStaffInvs")]
        //[DataScoped(true)]
        public IActionResult CreateStuffInv([FromBody] CreateStaffInvModel invStaffs)
        {
            if (invStaffs == null)
                throw new ArgumentException(nameof(invStaffs));
            var org = m_OrgManager.FindOrg(invStaffs.OrgId);
            using (var tx = TxManager.Acquire())
            {
                invStaffs.InviterName = this.AccountName;

                m_StaffInvManager.BulkCreateStuffInv(invStaffs);
                tx.Complete();

            }
            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "staffInv");
            extra.Add("OrgId", invStaffs.OrgId.ToString());

            m_NotificationManager.SendByAliasAsync(null,
                string.Format(m_Configuration["PushMessage:StaffInv:inv"], this.AccountName, org.Name),
                extra, invStaffs.Invitees.Select(p => p.Item2).ToArray());
            return new HttpStatusCodeResult((int) HttpStatusCode.Created);
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

            return new HttpStatusCodeResult(200);

        }
    }
}
