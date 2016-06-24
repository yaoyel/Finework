using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Net.Sms;
using FineWork.Security.Passwords;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Security
{
    [Route("api/Accounts")]
    [Authorize("Bearer")]
    public class AccountsController : FwApiController
    {
        public AccountsController(ISessionProvider<AefSession> sessionProvider,
            IAccountManager accountManager,
            IStaffManager staffManager,
            IOrgManager orgManager,
            ISmsService smsService,
            IHostingEnvironment hostEvn,
            IPasswordService passwordService
            )
            : base(sessionProvider)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (hostEvn == null) throw new ArgumentNullException(nameof(hostEvn));
            if (passwordService == null) throw new ArgumentNullException(nameof(passwordService));

            m_AccountManager = accountManager;
            m_StaffManager = staffManager;
            m_OrgManager = orgManager;
            m_SmsService = smsService;
            m_HostEvn = hostEvn;
            m_PasswordService = passwordService;
        }

        private readonly IAccountManager m_AccountManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IOrgManager m_OrgManager;
        private readonly ISmsService m_SmsService;
        private readonly IHostingEnvironment m_HostEvn;
        private readonly IPasswordService m_PasswordService;

        //[FwHandleErrors]
        [HttpPost("CreateAccount")]
        [AllowAnonymous]
        //[DataScoped(true)]
        public AccountViewModel CreateAccount([FromBody] CreateAccountModel accountModel)
        {
            if (accountModel == null) throw new ArgumentException(nameof(accountModel));

            var result = new AccountViewModel();
            using (var tx = TxManager.Acquire())
            {
                PhoneNumberAvailableResult.Check(this.m_AccountManager, accountModel.PhoneNumber).ThrowIfFailed();

                if (!string.IsNullOrEmpty(accountModel.Email))
                    EmailAvailableResult.Check(this.m_AccountManager, accountModel.Email, false);

                var account = (AccountEntity) m_AccountManager.CreateAccount(accountModel);

                result = account.ToViewModel();
                tx.Complete();
            }

            //异步设置默认头像

            Task.Factory.StartNew(() =>
            {
                var accountId = result.Id;
                var path = Path.Combine(m_HostEvn.WebRootPath, "Avatar", "default", "account.png");

                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    m_AccountManager.UploadAccountAvatar(fileStream, accountId, "Image/jpeg");
                }

            });

            return result;
        }

        [HttpPost("UploadAccountAvatar")]
        [AllowAnonymous]
        [IgnoreDataScoped]
        public void UploadAccountAvatar(IFormFile file, Guid? accountId)
        {
            if (file == null) throw new ArgumentException(nameof(file));
            if (!file.ContentType.StartsWith("image"))
                throw new FineWorkException("Unsupported file type!");

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                m_AccountManager.UploadAccountAvatar(reader.BaseStream, accountId ?? this.AccountId, file.ContentType);
            }
        }

        [HttpPost("ChangeAccountName")]
        //[DataScoped(true)]
        public IActionResult UpdateAccountName(string phoneNumber, string newAccountName)
        {
            if (string.IsNullOrEmpty(newAccountName)) throw new ArgumentException(nameof(newAccountName));
            IAccount account = null;

            using (var tx = TxManager.Acquire())
            {
                account = AccountNotExistsResult.Check(this.m_AccountManager, newAccountName).Account;
                if (account != null)
                {
                    if (account.Id != this.AccountId)
                        throw new FineWorkException($"已经存在名称为{newAccountName}的用户.");

                    return new NoContentResult();

                }

                account = !string.IsNullOrEmpty(phoneNumber)
                    ? m_AccountManager.FindAccountByPhoneNumber(phoneNumber)
                    : this.m_AccountManager.FindAccount(this.AccountId);
                account.Name = newAccountName;


                this.m_AccountManager.UpdateAccount(account);
                tx.Complete();

                return new HttpStatusCodeResult((int) HttpStatusCode.OK);
            }
        }

        [HttpGet("PhoneNumberIsAvailable")]
        [AllowAnonymous]
        public bool PhoneNumberIsAvailable(string phoneNumber)
        {
            return PhoneNumberAvailableResult.Check(this.m_AccountManager, phoneNumber).IsSucceed;
        }

        [AllowAnonymous]
        [HttpPost("VerifySmsCode")]
        public bool VerifySmsCode(string smsCode, string phoneNumber)
        {
            Args.NotNull(smsCode, nameof(smsCode));
            Args.NotNull(phoneNumber, nameof(phoneNumber));

            try
            {
                m_SmsService.VerifySmsCode(phoneNumber, smsCode);
            }
            catch
            {
                return false;
            }
            return true;
        }

        [AllowAnonymous]
        [HttpGet("RequestSmsCode")]
        public IActionResult RequestSmsCode(string phoneNumber)
        {
            m_SmsService.SendMessage(phoneNumber, "", null);
            return new HttpStatusCodeResult(200);
        }

        /// <summary> 根据 <see cref="IAccount.Id"/> 获取用户信息. </summary>
        [HttpGet("FindAccountById")]
        public AccountViewModel FindAccountById(Guid? accountId)
        {
            var account = m_AccountManager.FindAccount(accountId == null ? this.AccountId : accountId.Value);
            if (account == null) return null;
            return ((AccountEntity) account).ToViewModel();
        }

        [HttpPost("Validate")]
        public IActionResult ValidateAccount(string phoneNumber, string pwd)
        {
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber));
            if (string.IsNullOrEmpty(pwd)) throw new ArgumentException(nameof(pwd));

            var account = AccountExistsResult.CheckByPhoneNumber(m_AccountManager, phoneNumber).ThrowIfFailed().Account;

            if (!m_PasswordService.Verify(account.PasswordFormat, pwd, account.Password, account.PasswordSalt))
                return HttpUnauthorized();

            return new ObjectResult(((AccountEntity) account).ToViewModel());
        }

        [Route("FetchAccountsByOrgId")]
        public IEnumerable<AccountViewModel> FetchAccountsByOrgId(Guid orgId)
        {
            return m_StaffManager
                .FetchStaffsByOrg(orgId, true)
                .Select(p => p.Account)
                .Select(p => p.ToViewModel());
        }

        [HttpPost("ChangePassword")]
        [AllowAnonymous]
        public void ChangePassword(string phoneNumber, string newPwd)
        {
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber));
            using (var tx = TxManager.Acquire())
            {

                var account =
                    AccountExistsResult.CheckByPhoneNumber(m_AccountManager, phoneNumber).ThrowIfFailed().Account;

                this.m_AccountManager.ChangePassword(account, account.Password, newPwd);
                tx.Complete();
            }
        }
    }
}
