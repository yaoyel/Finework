using System;
using AppBoot.Common;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Logging;
using FineWork.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace FineWork.Security.Repos.Aef
{
    [TestFixture]
    public class LoginRepositoryTests : FineWorkCoreTestBase
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(AccountRepositoryTests));

        [Test]
        public void EntityTypes_are_correct()
        {
            this.Services.CheckRepositoryRegistered<ILoginRepository, ILogin, LoginEntity, Guid>();
        }

        [Test]
        public void Repository_supports_CRUD()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var sessionScope = this.Services.ResolveSessionProvider().GetSession())
                {
                    var accountRepository = this.Services.GetRequiredService<IAccountRepository>();
                    var account = accountRepository.CreateTestAccount(Guid.NewGuid());
                    accountRepository.Insert(account);

                    var loginRepository = this.Services.GetRequiredService<ILoginRepository>();

                    ILogin login = loginRepository.CreateTestLogin(Guid.NewGuid(), account);

                    loginRepository.Insert(login);

                    var loaded = loginRepository.Find(login.Id);
                    Assert.IsNotNull(loaded);

                    loginRepository.Delete(login);
                    var reloaded = loginRepository.Find(login.Id);
                    Assert.IsNull(reloaded);

                    accountRepository.Delete(account);

                    sessionScope.SaveChanges();
                }

                tx.Complete();
            }
        }
    }
}