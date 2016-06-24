using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using FineWork.Security.Repos.Aef;
using Microsoft.AspNet.Authorization;
using FineWork.Security.Checkers;
using FineWork.Security.Models;
using System.Net;
using AppBoot.Checks;
using FineWork.Common;
using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;
using System.IO;
using AppBoot.Common;
using FineWork.Web.WebApi.Core;

namespace FineWork.Web.WebApi.Security
{
    [Route("api/Accounts")]
    [Authorize("Bearer")] 
    public class AccountsController : FwApiController
    {
        public AccountsController(IAccountManager accountManager,
            IStaffManager staffManager,
            IOrgManager orgManager,
            ISessionScopeFactory sessionScopeFactory)
            :base(sessionScopeFactory)
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

        //[FwHandleErrors]
        [HttpPost("CreateAccount")]
        [AllowAnonymous]
        [DataScoped(true)]
        public AccountViewModel CreateAccount([FromBody]CreateAccountModel accountModel)
        {
            if (accountModel == null) throw new ArgumentException(nameof(accountModel));

            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                PhoneNumberAvailableResult.Check(this.m_AccountManager, accountModel.PhoneNumber).ThrowIfFailed();

                if (!string.IsNullOrEmpty(accountModel.Email))
                    EmailAvailableResult.Check(this.m_AccountManager, accountModel.Email, false);

                var account = (AccountEntity)m_AccountManager.CreateAccount(accountModel);
                sessionScope.SaveChanges();

                return account.ToViewModel();

            }
        }

        [HttpPost("UploadAccountAvatar")]
        [AllowAnonymous]
        [IgnoreDataScoped]
        public void UploadAccountAvatar(IFormFile file, Guid? accountId)
        {
            if (file == null) throw new ArgumentException(nameof(file));
            if (!file.ContentType.StartsWith("image"))
                throw new ArgumentException("Unsupported file type!");

            using (var reader = new StreamReader(file.OpenReadStream()))
            { 
                m_AccountManager.UploadAccountAvatar(reader.BaseStream,accountId ?? this.AccountId, file.ContentType);
            }   
        }

        [HttpPost("ChangeAccountName")]
        [DataScoped(true)]
        public IActionResult UpdateAccountName(string phoneNumber, string newAccountName)
        {
            if (string.IsNullOrEmpty(newAccountName)) throw new ArgumentException(nameof(newAccountName));
            IAccount account = null;
            if (!string.IsNullOrEmpty(phoneNumber))
                account = m_AccountManager.FindAccountByPhoneNumber(phoneNumber);
            else
                account = this.m_AccountManager.FindAccount(this.AccountId);
            account.Name = newAccountName;

            AccountNotExistsResult.Check(this.m_AccountManager, newAccountName).ThrowIfFailed();
            this.m_AccountManager.UpdateAccount(account);
            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
        }

        [HttpGet("PhoneNumberIsAvailable")]
        [AllowAnonymous]
        [DataScoped]
        public bool PhoneNumberIsAvailable(string phoneNumber)
        {
            return PhoneNumberAvailableResult.Check(this.m_AccountManager, phoneNumber).IsSucceed;
        }

        [AllowAnonymous]
        [HttpGet("VerificationCodeIsAvailable")]
        public bool VerificationCodeIsAvailable(string verificationCode)
        {
            return verificationCode == "1234";
        }

        [AllowAnonymous]
        [HttpGet("VerificationCode")]
        [IgnoreDataScoped]
        public string VerificationCode()
        {
            //TODO 
            return "1234";
        }

        /// <summary> 根据 <see cref="IAccount.Id"/> 获取用户信息. </summary>
        [HttpGet("FindAccountById")]
        [DataScoped]
        public AccountViewModel FindAccountById(Guid? accountId)
        {
            var account = m_AccountManager.FindAccount(accountId == null ? this.AccountId : accountId.Value);
            if (account == null) return null;
            return ((AccountEntity) account).ToViewModel();
        }

        [HttpPost("Validate")]
        [DataScoped]
        public IActionResult ValidateAccount(string phoneNumber, string pwd)
        {
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber));
            if (string.IsNullOrEmpty(pwd)) throw new ArgumentException(nameof(pwd));

            var account = m_AccountManager.FindAccountByPhoneNumber(phoneNumber);
            if (account == null || account.Password != pwd)
                return HttpUnauthorized();

            return new ObjectResult(((AccountEntity) account).ToViewModel());
        }

        [Route("FetchAccountsByOrgId")]
        [DataScoped]
        public IEnumerable<AccountViewModel> FetchAccountsByOrgId(Guid orgId)
        {
            return m_StaffManager
                .FetchStaffsByOrg(orgId)
                .Select(p => p.Account)
                .Select(p => p.ToViewModel());
        }
         
    }
}
