using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Colla.Impls;
using FineWork.Data.Aef;
using FineWork.Net.Mail;
using FineWork.Net.Sms;
using FineWork.Security;
using FineWork.Security.Impls;
using FineWork.Security.Passwords;
using FineWork.Security.Passwords.Impls;
using FineWork.Security.Repos;
using FineWork.Security.Repos.Aef;
using FineWork.Settings;
using FineWork.Settings.Repos;
using FineWork.Settings.Repos.Aef;
using NUnit.Framework;

namespace FineWork.Core
{
    [TestFixture]
    public class FineWorkServiceRegistrationTests : FineWorkCoreTestBase
    {
        [Test]
        public void Services_should_be_registered()
        {
            var services = this.Services;

            Checked<FineWorkDbContext, FineWorkDbContext>(services);
            Checked<ISessionProvider<AefSession>, ResolvableSessionProvider<FineWorkDbContext>>(services);
            Checked<IPasswordService, PasswordService>(services);
            Checked<IEmailService, NullEmailService>(services);
            Checked<ISmsService, NullSmsService>(services);

            Checked<ISettingRepository, SettingRepository>(services);
            Checked<IAccountRepository, AccountRepository>(services);
            Checked<ILoginRepository, LoginRepository>(services);
            Checked<IRoleRepository, RoleRepository>(services);
            Checked<IClaimRepository, ClaimRepository>(services);

            Checked<ISettingManager, SettingManager>(services);
            Checked<IAccountManager, AccountManager>(services);
            Checked<IRoleManager, RoleManager>(services);

            Checked<IOrgManager, OrgManager>(services);
            Checked<IStaffManager, StaffManager>(services);
            Checked<ITaskManager, TaskManager>(services);
            Checked<IPartakerManager, PartakerManager>(services);

            var partakerInvManager = Checked<IPartakerInvManager, PartakerInvManager>(services);
            Assert.NotNull(partakerInvManager.ReqManager);

            var partakerReqManager = Checked<IPartakerReqManager, PartakerReqManager>(services);
            Assert.NotNull(partakerReqManager);
        }

        private TImpl Checked<TDecl, TImpl>(IServiceProvider serviceProvider) where TImpl : TDecl
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            TDecl service = (TDecl)serviceProvider.GetService(typeof (TDecl));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<TImpl>(service);
            return (TImpl) service;
        }
    }
}
