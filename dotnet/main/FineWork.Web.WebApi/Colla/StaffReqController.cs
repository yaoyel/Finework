using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using System.Net;
using Microsoft.AspNet.Authorization;
using FineWork.Colla.Checkers;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Message;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/StaffReqs")]
    [Authorize("Bearer")]
    public class StaffReqController : FwApiController
    {
        public StaffReqController(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IOrgManager orgManager,
            IAccountManager accountManager,
            IStaffReqManager staffReqManager,
            INotificationManager notificationManager,
            IConfiguration config)
            : base(sessionProvider)
        {
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (staffReqManager == null) throw new ArgumentException(nameof(staffReqManager));
            if (accountManager == null) throw new ArgumentException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));
            if (config == null) throw new ArgumentException(nameof(config));
            m_StaffManager = staffManager;
            m_StaffReqManager = staffReqManager;
            m_OrgManager = orgManager;
            m_NotificationManager = notificationManager;
            m_AccountManager = accountManager;
            m_Configuration = config;

        }

        private IConfiguration m_Configuration;

        private readonly IStaffReqManager m_StaffReqManager;
        private readonly IOrgManager m_OrgManager;
        private readonly IStaffManager m_StaffManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IAccountManager m_AccountManager;

        [HttpPost("CreateStaffReq")]
        //[DataScoped(true)]
        public StaffReqViewModel CreateStaffReq(Guid orgId, string message)
        {
            using (var tx = TxManager.Acquire())
            {
                StaffReqViewModel staffReq = null;
                var org = m_OrgManager.FindOrg(orgId);
                staffReq = m_StaffReqManager.CreateStaffReq(this.AccountId, orgId, message).ToViewModel();

                //如果存在管理员，发通知给管理员
                if (org.AdminStaff != null)
                {
                    var extra = new Dictionary<string, string>();
                    extra.Add("PathTo", "staffReq");
                    extra.Add("OrgId", orgId.ToString());

                    var notification = string.Format(m_Configuration["PushMessage:StaffReq:req"],
                        this.AccountName, org.Name);
                    m_NotificationManager.SendByAliasAsync(null, notification, extra,
                        org.AdminStaff.Account.PhoneNumber).Wait();
                }
                tx.Complete();
                return staffReq;
            }
        }

        [HttpPost("UpdateReviewStatus")]
        //[DataScoped(true)]
        public IActionResult ChangeReviewStatus(Guid staffReqId, short newStatus)
        {
            using (var tx = TxManager.Acquire())
            {
                var staffReq = m_StaffReqManager.FindStaffReqById(staffReqId);

                //判断当前用户是否有管理员权限
                PermissionIsAdminResult.Check(m_StaffManager, staffReq.Org.Id, this.AccountId)
                    .ThrowIfFailed();
                m_StaffReqManager.ChangeReviewStatus(staffReq, (ReviewStatuses) newStatus);


                var extra = new Dictionary<string, string>();
                extra.Add("PathTo", "reviewStaffReq");
                extra.Add("status", newStatus.ToString());

                //推送消息给申请人员
                var message = string.Format(m_Configuration["PushMessage:StaffReq:review"],
                    staffReq.Org.Name, newStatus == (short) ReviewStatuses.Approved ? "同意" : "拒绝");

                m_NotificationManager.SendByAliasAsync(null, message, extra, staffReq.Account.PhoneNumber).Wait();
                tx.Complete();
                return new HttpStatusCodeResult(200);
            }
        }

        [HttpGet("FetchStaffReqsByOrg")]
        public IEnumerable<StaffReqViewModel> FetchStaffReqsByOrg(Guid orgId)
        {
            return m_StaffReqManager.FetchStaffReqsByOrg(orgId)
                .Select(p => p.ToViewModel());
        }

        [HttpGet("FetchStaffReqsByAccount")]
        public IEnumerable<StaffReqViewModel> FetchStaffReqsByAccount()
        {
            return m_StaffReqManager.FetchStaffReqsByAccount(this.AccountId)
                .Select(p => p.ToViewModel());
        }

    }
}