using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using System.Net;
using Microsoft.AspNet.Authorization;
using FineWork.Colla.Checkers;
using AppBoot.Checks;
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
        public StaffReqController(ISessionScopeFactory sessionScopeFactory,
            IStaffManager staffManager,
            IOrgManager orgManager,
            IAccountManager accountManager,
            IStaffReqManager staffReqManager,
            INotificationManager notificationManager,
            IApplicationEnvironment appEnv)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentException(nameof(sessionScopeFactory));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (staffReqManager == null) throw new ArgumentException(nameof(staffReqManager));
            if (accountManager == null) throw new ArgumentException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));

            m_SessionScopeFactory = sessionScopeFactory;
            m_StaffManager = staffManager;
            m_StaffReqManager = staffReqManager;
            m_OrgManager = orgManager;
            m_NotificationManager = notificationManager;
            m_AccountManager = accountManager;
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath);
            configBuilder
                .AddJsonFile(@"Configs\pushSetting.json", false);
            Configuration = configBuilder.Build();

        }

        public IConfiguration Configuration { get; set; }

        private readonly ISessionScopeFactory m_SessionScopeFactory;
        private readonly IStaffReqManager m_StaffReqManager;
        private readonly IOrgManager m_OrgManager;
        private readonly IStaffManager m_StaffManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IAccountManager m_AccountManager;

        [HttpPost("CreateStaffReq")]
        [DataScoped(true)]
        public StaffReqViewModel CreateStaffReq(Guid orgId, string message)
        {

            StaffReqViewModel staffReq = null;
            var org = m_OrgManager.FindOrg(orgId);
            staffReq = m_StaffReqManager.CreateStuffReq(this.AccountId, orgId, message).ToViewModel();

            //如果存在管理员，发通知给管理员
            if (org.AdminStaff != null)
            {
                var extra = new Dictionary<string, string>();
                extra.Add("PathTo", "staffReq");

                var notification = string.Format(Configuration.Get("PushMessage:StaffReq:description"),
                    this.AccountName, org.Name);
                m_NotificationManager.SendByAliasAsync(null, notification, extra,
                    org.AdminStaff.Account.PhoneNumber).Wait();
            }
            return staffReq;
        }

        [HttpPost("UpdateReviewStatus")]
        [DataScoped(true)]
        public IActionResult ChangeReviewStatus(Guid staffReqId, short newStatus)
        {
            var staffReq = m_StaffReqManager.FindStaffReqById(staffReqId);

            //判断当前用户是否有管理员权限
            PermissionIsAdminResult.Check(m_StaffManager, staffReq.Org.Id, this.AccountId)
                .ThrowIfFailed();
            var staff = m_StaffReqManager.ChangeReviewStatus(staffReq, (ReviewStatuses) newStatus);

            //推送消息给申请人员
            var message = string.Format(Configuration.Get("PushMessage:StaffReq:review"),
                staffReq.Org.Name, newStatus == (short) ReviewStatuses.Approved ? "同意" : "拒绝");

            m_NotificationManager.SendByAliasAsync(null, message, null, staff.Account.PhoneNumber).Wait();

            return new ObjectResult(staff);

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