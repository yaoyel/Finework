using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Impls;
using FineWork.Security;
using FineWork.Web.WebApp.Models;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FineWork.Web.WebApp.ApiControllers
{
    [Route("api/org")]
    public class OrgController : Controller
    {
        public OrgController(ISessionScopeFactory sessionScopeFactory,
            IStaffManager staffManager,
            IOrgManager orgManager,
            IAccountManager accountManager)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));

            m_SessionScopeFactory = sessionScopeFactory;
            m_OrgManager = orgManager;
            m_StaffManager = staffManager;
            m_AccountManager = accountManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;

        private readonly IOrgManager m_OrgManager;

        private readonly IStaffManager m_StaffManager;

        private readonly IAccountManager m_AccountManager;


        /// <summary>
        /// 获取所有组织机构信息
        /// </summary>
        /// <returns></returns>
        [Route("FetchOrgs")]
        public IEnumerable<OrgViewModel> FetchOrgs()
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var orgs = m_OrgManager.FetchOrgs();
                return orgs.Select(org => new OrgViewModel { Id = org.Id, Name = org.Name });

            }
        }

         [HttpGet("fetchbyaccount")]
        public IEnumerable<OrgViewModel> FetchOrgsByAccountId(Guid accountId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var orgs = m_StaffManager.FetchStaffsByAccount(accountId)
                    .Select(p => p.Org).ToList();
                return orgs.Select(p => new OrgViewModel()
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList();
            }
        }

    }
}
