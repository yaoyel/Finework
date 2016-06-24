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
    public class ClaimRepositoryTests : FineWorkCoreTestBase
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(AccountRepositoryTests));

        [Test]
        public void EntityTypes_are_correct()
        {
            this.Services.CheckRepositoryRegistered<IClaimRepository, IClaim, ClaimEntity, Guid>();
        }

        [Test]
        public void Repository_supports_CRUD()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var accountRepository = this.Services.GetRequiredService<IAccountRepository>();
                    var account = accountRepository.CreateTestAccount(Guid.NewGuid());
                    accountRepository.Insert(account);

                    var claimRepository = this.Services.GetRequiredService<IClaimRepository>();

                    IClaim claim = claimRepository.CreateTestClaim(Guid.NewGuid(), account);

                    claimRepository.Insert(claim);

                    var loaded = claimRepository.Find(claim.Id);
                    Assert.IsNotNull(loaded);

                    claimRepository.Delete(claim);
                    var reloaded = claimRepository.Find(claim.Id);
                    Assert.IsNull(reloaded);

                    accountRepository.Delete(account);

                    session.SaveChanges();
                }

                tx.Complete();
            }
        }
    }
}
