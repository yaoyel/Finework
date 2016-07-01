using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Security.Passwords;
using FineWork.Common;
using FineWork.Security.Checkers;
using FineWork.Security.Passwords;
using FineWork.Security.Repos;
using FineWork.Settings;
using JetBrains.Annotations;
using FineWork.Security.Models;
using System.IO;
using AppBoot.Common;
using FineWork.Avatar;

namespace FineWork.Security.Impls
{
    public class AccountManager : IAccountManager
    {
        public AccountManager(
            IAccountRepository accountRepository, 
            ILoginRepository loginRepository, 
            ISettingManager settingManager, 
            IPasswordService passwordService,
            IAvatarManager avatarManager
            )
        {
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");
            if (loginRepository == null) throw new ArgumentNullException("loginRepository");
            if (settingManager == null) throw new ArgumentNullException("settingManager");
            if (passwordService == null) throw new ArgumentNullException("passwordService");
            if (avatarManager == null) throw new ArgumentNullException("avatarManager");

            m_SettingManager = settingManager;
            m_PasswordService = passwordService;
            m_AccountRepository = accountRepository;
            m_LoginRepository = loginRepository;
            AvatarManager = avatarManager;
        }

        private readonly ISettingManager m_SettingManager;

        protected ISettingManager SettingManager
        {
            get { return m_SettingManager; }
        }

        private readonly IAccountRepository m_AccountRepository;

        protected IAccountRepository AccountRepository
        {
            get { return m_AccountRepository; }
        }

        private readonly ILoginRepository m_LoginRepository;

        protected ILoginRepository LoginRepository
        {
            get { return m_LoginRepository; }
        }

        private readonly IPasswordService m_PasswordService;

        protected IPasswordService PasswordService
        {
            get { return m_PasswordService; }
        }

        protected IAvatarManager AvatarManager { get; }

        public IAccount CreateAccount(CreateAccountModel createAccountModel)
        {
            if (createAccountModel == null) throw new ArgumentNullException(nameof(createAccountModel));
            var account = InternalCreateAccount(createAccountModel);
            return account;
        }

        public IAccount CreateAccount(string name, string password, string email, string phoneNumber)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("name is null or empty.", nameof(name));
            //if (String.IsNullOrEmpty(email)) throw new ArgumentException("email is null or empty.", nameof(email));
            //if (String.IsNullOrEmpty(phoneNumber)) throw new ArgumentException("phoneNumber is null or empty.", nameof(phoneNumber));

            var account = AccountRepository.CreateNew();
            account.Name = name;
            account.Email = email;
            account.PhoneNumber = phoneNumber;
            account.IsEmailConfirmed = false;
            account.IsPhoneNumberConfirmed = false;
            account.SecurityStamp = GenerateNewStamp(account);
            account.CreatedAt = DateTime.Now;

            AccountRepository.Insert(account);
            return account;
        }

        protected IAccount InternalCreateAccount(CreateAccountModel createAccountModel)
        { 
            var account = AccountRepository.CreateNew();
            account.Name = createAccountModel.Name;
            account.Email = createAccountModel.Email;
            account.PhoneNumber = createAccountModel.PhoneNumber;
            account.IsEmailConfirmed = false;
            account.IsPhoneNumberConfirmed = false;   
            this.PasswordService.SetPassword(account, SettingManager.PasswordFormat(), createAccountModel.Password);
            account.SecurityStamp = GenerateNewStamp(account);
            AccountRepository.Insert(account);

            return account;
        }

        public void UpdateAccount(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account"); 
            AccountRepository.Update(account);
        }

        public void DeleteAccount(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            //Delete associated logins
            foreach (var login in account.Logins.ToArray())
            {
                LoginRepository.Delete(login);
            }

            AccountRepository.Delete(account);
        }

        public void ReloadAccount(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            AccountRepository.Reload(account);
        }

        public IAccount FindAccount(Guid id)
        {
            return AccountRepository.Find(id);
        }

        public IAccount FindAccountByName(String accountName)
        {
            if (String.IsNullOrEmpty(accountName)) throw new ArgumentNullException("accountName");

            return this.AccountRepository.Fetch(x => x.Name == accountName).SingleOrDefault();
        }

        public IAccount FindAccountByPhoneNumber(string phoneNumber)
        {
            if(string.IsNullOrEmpty(phoneNumber)) throw  new ArgumentException("phoneNumber");

            return this.AccountRepository.Fetch(x => x.PhoneNumber == phoneNumber).SingleOrDefault();
        }

        public ICollection<IAccount> FetchAccountsByEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException("email");

            return this.AccountRepository.Fetch(x => x.Email == email);
        }

        public ICollection<IAccount> FetchAccounts()
        {
            return AccountRepository.FetchAll();
        }

        public void ChangePassword(IAccount account, [CanBeNull] String oldPassword, [CanBeNull] String newPassword)
        {
            if (account == null) throw new ArgumentNullException("account");

            if (!string.IsNullOrWhiteSpace(oldPassword))
                PasswordMatchResult.Check(this.PasswordService, account, oldPassword).ThrowIfFailed();

            var passwordFormat = account.PasswordFormat;
            if (String.IsNullOrEmpty(newPassword))
            {
                PasswordMayEmptyResult.Check(this, account).ThrowIfFailed();
                passwordFormat = PasswordFormats.NoPassword;
            }
            this.PasswordService.SetPassword(account, passwordFormat, newPassword);
            this.UpdateAccountWithNewStamp(account);
        }

        public void ConfirmEmail(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (!account.IsEmailConfirmed)
            {
                account.IsEmailConfirmed = true;
                this.UpdateAccountWithNewStamp(account);
            }
        }

        public ILogin CreateAccountWithLogin(string accountName, string email, string provider, string providerKey)
        {
            if (String.IsNullOrEmpty(accountName)) throw new ArgumentNullException("accountName");
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException("email");
            if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");

            AccountNotExistsResult.Check(this, accountName).ThrowIfFailed();
            EmailAvailableResult.Check(this, email, this.SettingManager.IsEmailReuseAllowed()).ThrowIfFailed();
            LoginNotExistsResult.Check(this, provider, providerKey).ThrowIfFailed();

            var account = InternalCreateAccount(new CreateAccountModel() {
                Name=accountName,
                Email=email,
                Password=null
            });
            var login = InternalCreateLogin(account, provider, providerKey);
            return login;
        }

        public ILogin CreateLogin(IAccount account, String provider, String providerKey)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");

            LoginNotExistsResult.Check(this, provider, providerKey).ThrowIfFailed();

            var login = InternalCreateLogin(account, provider, providerKey);
            this.UpdateAccountWithNewStamp(account);
            return login;
        }

        private ILogin InternalCreateLogin(IAccount account, String provider, String providerKey)
        {
            if (account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");

            var login = this.LoginRepository.CreateNew();
            login.Account = account;
            login.Provider = provider;
            login.ProviderKey = providerKey;
            account.Logins.Add(login);
            this.LoginRepository.Insert(login);
            return login;
        }

        public void DeleteLogin(Guid loginId)
        {
            var login = this.FindLogin(loginId);
            if (login == null) throw new ArgumentException(String.Format("The login for Id [{0}] does not exist.", loginId));

            bool isLastLogin = login.Account.Logins.Count == 1;
            bool noLocalPassword = String.IsNullOrEmpty(login.Account.Password);
            if (isLastLogin && noLocalPassword)
            {
                throw new FineWorkException("Cannot remove the last external login when the account has no local password.");
            }
            this.LoginRepository.Delete(login);
            this.UpdateAccountWithNewStamp(login.Account);
        }

        public void ReloadLogin(ILogin login)
        {
            if (login == null) throw new ArgumentNullException("login");

            this.LoginRepository.Reload(login);
        }

        public ILogin FindLogin(Guid loginId)
        {
            return LoginRepository.Find(loginId);
        }

        public ILogin FindLogin(String provider, String providerKey)
        {
            return LoginRepository.Find(provider, providerKey);
        }

        protected void UpdateAccountWithNewStamp(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            account.SecurityStamp = GenerateNewStamp(account);
            this.AccountRepository.Update(account);
        }

        private String GenerateNewStamp(IAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            return Guid.NewGuid().ToString("D");
        }

        public void UploadAccountAvatar(Stream stream, Guid accountId, string contentType)
        {
            AvatarManager.CreateAvatars(KnownAvatarOwnerTypes.Accounts, accountId, stream);
            var account = AccountExistsResult.Check(this, accountId).ThrowIfFailed().Account;
            UpdateAccountWithNewStamp(account);
        }

        public void ChangeAccountName(Guid accountId, string newAccountName)
        {
            Args.NotEmpty(newAccountName, nameof(newAccountName));

            var account = AccountExistsResult.Check(this, accountId).ThrowIfFailed().Account; 

            account.Name = newAccountName;
            this.UpdateAccount(account);
        }
    }
}
