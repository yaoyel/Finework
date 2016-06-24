using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using FineWork.Security.Repos;
using Microsoft.AspNet.Identity;

namespace FineWork.Security.Identity
{
    public class AspNetIdentityUserStoreImpl : DisposableBase,
        IUserStore<IAccount, Guid>,
        IQueryableUserStore<IAccount, Guid>,
        IUserPasswordStore<IAccount, Guid>,
        IUserSecurityStampStore<IAccount, Guid>,
        IUserEmailStore<IAccount, Guid>,
        IUserPhoneNumberStore<IAccount, Guid>,
        IUserTwoFactorStore<IAccount, Guid>,
        IUserLockoutStore<IAccount, Guid>,
        IUserLoginStore<IAccount, Guid>,
        IUserRoleStore<IAccount, Guid>,
        IUserClaimStore<IAccount, Guid>
    {
        public AspNetIdentityUserStoreImpl(IAccountRepository accountRepository,
            ILoginRepository loginRepository,
            IClaimRepository claimRepository,
            IRoleRepository roleRepository)
        {
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");
            if (loginRepository == null) throw new ArgumentNullException("loginRepository");
            if (claimRepository == null) throw new ArgumentNullException("claimRepository");
            if (roleRepository == null) throw new ArgumentNullException("roleRepository");
            m_AccountRepository = accountRepository;
            m_LoginRepository = loginRepository;
            m_ClaimRepository = claimRepository;
            m_RoleRepository = roleRepository;
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

        private readonly IClaimRepository m_ClaimRepository;

        protected IClaimRepository ClaimRepository
        {
            get { return m_ClaimRepository; }
        }

        private readonly IRoleRepository m_RoleRepository;

        protected IRoleRepository RoleRepository
        {
            get { return m_RoleRepository; }
        }

        protected override void DoDispose(bool disposing)
        {
            base.DoDispose(disposing);
        }

        #region IUserStore

        public Task CreateAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return this.AccountRepository.InsertAsync(user);
        }

        public Task UpdateAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return this.AccountRepository.UpdateAsync(user);
        }

        public Task DeleteAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return this.AccountRepository.DeleteAsync(user);
        }

        public Task<IAccount> FindByIdAsync(Guid userId)
        {
            return this.AccountRepository.FindAsync(userId);
        }

        public async Task<IAccount> FindByNameAsync(string userName)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("userName is null or empty.", "userName");
            var list = await this.AccountRepository.FetchAsync(x => x.Name == userName);
            return list.SingleOrDefault();
        }

        #endregion

        #region IQueryableUserStore

        public IQueryable<IAccount> Users
        {
            //TODO: Possible IDbRepository.GetQueryable
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IUserPasswordStore

        public Task SetPasswordHashAsync(IAccount user, string passwordHash)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.Password = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(IAccount user)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(!String.IsNullOrEmpty(user.Password));
        }

        #endregion

        #region IUserSecurityStampStore

        public Task SetSecurityStampAsync(IAccount user, string stamp)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<String> GetSecurityStampAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion

        #region IUserEmailStore

        public Task<string> GetEmailAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.Email);
        }

        public Task SetEmailAsync(IAccount user, string email)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<bool> GetEmailConfirmedAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.IsEmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(IAccount user, bool confirmed)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.IsEmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<IAccount> FindByEmailAsync(string email)
        {
            return Task.Run(() => this.AccountRepository.Fetch(x => x.Email == email).SingleOrDefault());
        }

        #endregion

        #region IUserPhoneNumberStore

        public Task<string> GetPhoneNumberAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PhoneNumber);
        }

        public Task SetPhoneNumberAsync(IAccount user, string phoneNumber)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.IsPhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(IAccount user, bool confirmed)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.IsEmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserTwoFactorStore

        public Task<bool> GetTwoFactorEnabledAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.IsTwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(IAccount user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.IsTwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserLockoutStore

        public Task<bool> GetLockoutEnabledAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.IsLocked);
        }

        public Task SetLockoutEnabledAsync(IAccount user, bool enabled)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.IsLocked = enabled;
            return Task.FromResult(0);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.LockEndUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(user.LockEndUtc.Value, DateTimeKind.Utc))
                : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(IAccount user, DateTimeOffset lockoutEnd)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.LockEndUtc = lockoutEnd == DateTimeOffset.MinValue ? (DateTime?)null : lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PasswordFailedCount++;
            return Task.FromResult(user.PasswordFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.PasswordFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return Task.FromResult(user.PasswordFailedCount);
        }

        #endregion

        #region IUserLoginStore

        public Task AddLoginAsync(IAccount user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");

            ILogin loginEntity = login.ToLogin(this.LoginRepository);
            loginEntity.Account = user;
            return this.LoginRepository.InsertAsync(loginEntity);
        }

        public async Task RemoveLoginAsync(IAccount user, UserLoginInfo login)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (login == null) throw new ArgumentNullException("login");

            var loginEntity = (await this.LoginRepository.FetchAsync(x => x.Provider == login.LoginProvider && x.ProviderKey == login.ProviderKey))
                .SingleOrDefault();
            if (loginEntity == null) throw new InvalidOperationException("The login not found.");
            await this.LoginRepository.DeleteAsync(loginEntity);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");

            IList<UserLoginInfo> logins = user.Logins.Select(entity => entity.ToLoginInfo()).ToList();
            return Task.FromResult(logins);
        }

        public Task<IAccount> FindAsync(UserLoginInfo login)
        {
            if (login == null) throw new ArgumentNullException("login");
            return Task.Run(() => this.LoginRepository.FindAccount(login.LoginProvider, login.ProviderKey));
        }

        #endregion

        #region IUserRoleStore

        public async Task AddToRoleAsync(IAccount user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentException("roleName is null or empty.", "roleName");

            IRole role = this.RoleRepository.Create();
            role.Name = roleName;
            user.Roles.Add(role);
            await this.RoleRepository.InsertAsync(role);
        }

        public async Task RemoveFromRoleAsync(IAccount user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentException("roleName is null or empty.", "roleName");

            IRole role = user.Roles.SingleOrDefault(x => x.Name == roleName);
            if (role != null)
            {
                user.Roles.Remove(role);
                await this.RoleRepository.DeleteAsync(role);
            }
        }

        public Task<IList<string>> GetRolesAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var result = user.Roles.Select(x => x.Name).ToList() as IList<String>;
            return Task.FromResult(result);
        }

        public Task<bool> IsInRoleAsync(IAccount user, string roleName)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentException("roleName is null or empty.", "roleName");

            bool result = user.Roles.FirstOrDefault(x => x.Name == roleName) != null;
            return Task.FromResult(result);
        }

        #endregion

        #region IUserClaimStore

        public Task<IList<Claim>> GetClaimsAsync(IAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");
            IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult(result);
        }

        public Task AddClaimAsync(IAccount user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");

            IClaim claimEntity = this.ClaimRepository.Create();
            claimEntity.Account = user;
            claimEntity.ClaimType = claim.Type;
            claimEntity.ClaimValue = claim.Value;
            user.Claims.Add(claimEntity);
            return this.ClaimRepository.InsertAsync(claimEntity);
        }

        public async Task RemoveClaimAsync(IAccount user, Claim claim)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (claim == null) throw new ArgumentNullException("claim");
            var list = this.ClaimRepository.Fetch(
                x => x.Account == user && x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            foreach (var c in list)
            {
                await this.ClaimRepository.DeleteAsync(c);
            }
        }

        #endregion
    }
}
