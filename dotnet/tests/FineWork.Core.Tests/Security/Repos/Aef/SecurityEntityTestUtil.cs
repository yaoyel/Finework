using System;
using AppBoot.Repos;
using FineWork.Security.Passwords;

namespace FineWork.Security.Repos.Aef
{
    public static class SecurityEntityTestUtil
    {
        public static IAccount CreateTestAccount(this IAccountRepository repository, Guid id)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            var now = DateTime.Now;

            IAccount account = repository.Create(id);
            account.Name = "Account-" + id;
            account.Email = account.Name + "@example.com";
            account.Password = "password";
            account.PasswordFormat = PasswordFormats.ClearText;
            account.IsLocked = false;

            return account;
        }

        public static ILogin CreateTestLogin(this ILoginRepository repository, Guid id, IAccount account)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            ILogin login = repository.Create(id);
            login.Account = account;
            login.Provider = "TestProvider";
            login.ProviderKey = id.ToString("D");
            return login;
        }

        public static IClaim CreateTestClaim(this IClaimRepository repository, Guid id, IAccount account)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            IClaim claim = repository.Create(id);
            claim.Account = account;
            claim.ClaimType = "TestType";
            claim.ClaimValue = id.ToString("D");
            return claim;
        }
    }
}