using System;
using System.Configuration;
using System.IO;
using AppBoot.Transactions;
using FineWork.Azure;
using FineWork.Colla;
using FineWork.Core;
using FineWork.Files;
using FineWork.Message;
using FineWork.Security;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Notification
{
    [TestFixture]

    public class NotificationManagerTests : FineWorkCoreTestBase
    {
        private const string m_Message = "notification_test_message";
        private const string m_Alias = "13705189392"; 
        private const string m_NotExistsAlias = "11111111111";

        [Test]
        public void send_by_registrationId_async_return_success()
        {
            var notificationManager = this.Services.GetRequiredService<INotificationManager>();
            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {  
                    var device = notificationManager.FindDeviceRegistraionByAccountIdAsync(new Guid("C03092DE-5507-454F-A9ED-44CECAD2FC16")).Result;
                    Assert.NotNull(device); 

                    var result=notificationManager.SendAsync(null, m_Message, null, device.RegistrationId).Result;
                    Assert.IsTrue(result.IsSuccess);
                }
                tx.NoComplete();
            }
        }

        [Test]
        public void send_by_alias_async_return_failed_if_alias_is_not_exist()
        {
            var notificationManager = this.Services.GetRequiredService<INotificationManager>();
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                { 
                    var result = notificationManager.SendByAliasAsync(null, m_Message, null,m_Alias).Result;
                    Assert.IsTrue(result.IsSuccess);

                    var failedResult= notificationManager.SendByAliasAsync(null, m_Message, null, m_NotExistsAlias).Result;
                    Assert.IsFalse(failedResult.IsSuccess);
                }
                tx.NoComplete();
            }
        }
    }
}