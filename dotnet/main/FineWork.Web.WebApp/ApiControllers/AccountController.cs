using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Web.WebApp.Models;
using FineWork.Core;
using FineWork.Security;
namespace FineWork.Web.WebApp.ApiControllers
{
    [Route(m_RoutePrefix)]
    public class AccountController : Controller
    {
        private const String m_RoutePrefix = "api/Account";

        public AccountController(IAccountManager accountManager,
            IStaffManager staffManager,
            IOrgManager orgManager,
            ISessionScopeFactory sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));

            m_AccountManager = accountManager;
            m_SessionScopeFactory = sessionScopeFactory;
            m_StaffManager = staffManager;
            m_OrgManager = orgManager;
        }

        private readonly IAccountManager m_AccountManager;
        private readonly IStaffManager m_StaffManager;
        private readonly ISessionScopeFactory m_SessionScopeFactory;
        private readonly IOrgManager m_OrgManager;

        /// <summary>
        /// 根据id获取用户信息
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Route("{id:guid}")]
        [HttpGet("FindAccountById")]
        public AccountViewModel FindAccountById(Guid accountId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var account = m_AccountManager.FindAccount(accountId);
                if (account == null) return null;
                return new AccountViewModel()
                {
                    Id = account.Id,
                    Email = account.Email,
                    Name = account.Name,
                    PhoneNumber = account.PhoneNumber
                };
            }
        }

        [Route("validate")]
        [HttpPost]
        public IActionResult ValidateAccount(string phoneNumber, string pwd)
        {
            if(string.IsNullOrEmpty(phoneNumber)) throw  new ArgumentException(nameof(phoneNumber));

            if(string.IsNullOrEmpty(pwd)) throw new ArgumentException(nameof(pwd));

            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var account = m_AccountManager.FindAccountByPhoneNumber(phoneNumber);
                if (account == null || account.Password != pwd)
                    return HttpUnauthorized();

                return new ObjectResult(new AccountViewModel()
                {
                    Id = account.Id,
                    Email = account.Email,
                    Name = account.Name,
                    PhoneNumber = account.PhoneNumber
                });
            }
        }

        [Route("listbyorg/{id:guid}")] 
        public IEnumerable<AccountViewModel> FetchAccountByOrg(Guid orgId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                return m_StaffManager
                    .FetchStaffsByOrg(orgId)
                    .Select(p => p.Account)
                    .Select(p => new AccountViewModel()
                    {
                        Id = p.Id,
                        Email = p.Email,
                        Name = p.Name,
                        PhoneNumber = p.PhoneNumber
                    });
            }
        }

    }
}
