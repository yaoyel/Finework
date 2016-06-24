using System;
using FineWork.Security.Repos.Aef;
using FineWork.Web.WebApi.Common;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace FineWork.Web.WebApi.Tests.Core
{
    [Collection(FwApiBed.Name)]
    public class FwApiTestBaseTests : FwApiTestBase
    {
        public FwApiTestBaseTests(FwApiBed bed) 
            : base(bed)
        {
        }

        [Fact]
        public void InternalSetAccount_sets_User()
        {
            FoosController c = new FoosController(null);
            var account = new AccountEntity();
            account.Id = Guid.NewGuid();
            account.Name = "InternalSetAccount_sets_User";
            account.PhoneNumber = FakePhoneNumber;

            c.InternalSetAccount(account);
            Assert.True(c.User.Identity.IsAuthenticated);
        }
    }
}