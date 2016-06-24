using System;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Repos;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Security.Repos.Aef
{
     [TestFixture]
    public class RoleRepositoryTests : FineWorkCoreTestBase
    {
         [Test]
         public void EntityTypes_are_correct()
         {
             this.Services.CheckRepositoryRegistered<IRoleRepository, IRole, RoleEntity, Guid>();
         }

         [Test]
        public void Repository_supports_CRUD()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var repository = this.Services.GetRequiredService<IRoleRepository>();

                    IRole role = repository.CreateNew();
                    role.Name = "Mary";
                    
                    repository.Insert(role);

                    var loaded = repository.Find(role.Id);
                    Assert.IsNotNull(loaded);

                    repository.Delete(role);
                    var reloaded = repository.Find(role.Id);
                    Assert.IsNull(reloaded);

                    session.SaveChanges();
                }

                tx.Complete();
            }
        }
    }
}
