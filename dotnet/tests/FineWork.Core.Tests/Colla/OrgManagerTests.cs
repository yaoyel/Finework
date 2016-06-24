using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Security;
using FineWork.Security.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Colla
{
    [TestFixture]
    public class OrgManagerTests : FineWorkCoreTestBase
    {
        [Test]
        public void CreateOrg_creates_org()
        {
            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            var orgManager = this.Services.GetRequiredService<IOrgManager>();
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var adminAccount = accountManager.CreateAccount(new CreateAccountModel()
                    {
                        Name = "Test-Account-001",
                        PhoneNumber = "11111111111",
                        Password = "123456",
                        ConfirmPassword = "123456",
                        Email = "Test-Account-001@example.com"
                    });

                    var orgName = "Test-Org-001";
                    var org = orgManager.CreateOrg(adminAccount, orgName, null);
                    session.SaveChanges();

                    var byId = orgManager.FindOrg(org.Id);
                    Assert.NotNull(byId);
                    var byName = orgManager.FindOrgByName(orgName);
                    Assert.AreSame(byId, byName);

                    var list = orgManager.FetchOrgs();
                    Assert.IsTrue(list.Contains(org));
                }
                
                tx.NoComplete();
            }
        }

        [Test]
        public void ChangeAdmin_changes_org_admin()
        {
            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            var orgManager = this.Services.GetRequiredService<IOrgManager>();
            var staffManager = this.Services.GetRequiredService<IStaffManager>();
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var adminAccount = accountManager.CreateAccount(new CreateAccountModel()
                    {
                        Name = "Test-Account-001",
                        PhoneNumber = "11111111111",
                        Password = "123456",
                        ConfirmPassword = "123456",
                        Email = "Test-Account-001@example.com"
                    });

                    var orgName = "Test-Org-001";
                    var org = orgManager.CreateOrg(adminAccount, orgName, null);
                    var adminStaff = staffManager.CreateStaff(org.Id, adminAccount.Id, adminAccount.Name);
                    orgManager.ChangeOrgAdmin(org, adminStaff.Id);
                    session.SaveChanges();
                }

                tx.NoComplete();
            }
        }

    }
}
