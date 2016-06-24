using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Security;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Colla
{
    [TestFixture]
    public class MomentManagerTests:FineWorkCoreTestBase
    {
        public void CreateMoment_diff_type()
        {
            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            var orgManager = this.Services.GetRequiredService<IOrgManager>();
            var staffManager = this.Services.GetRequiredService<IStaffInvManager>();
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var adminAccount = accountManager.CreateAccount(new Security.Models.CreateAccountModel()
                    {
                        Name="TestMomentAccount-001",
                        PhoneNumber="11111111111",
                        Password="123456",
                        ConfirmPassword="123456",
                        Email="TestMoment-Account-001@example.com"
                    });
                    var orgName = "Test-Org-001";
                    var org=orgManager.CreateOrg(adminAccount, orgName, null);
                  
                }
            }
        }
    }
}
