using System;
using System.Configuration;
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
using Microsoft.Extensions.DependencyInjection;

namespace FineWork.Core
{
    /// <summary>
    /// <see cref="FineWorkCoreTestBase"/> registers a group of common services defined in <see cref="FineWork.Core"/>
    /// </summary>
    public class FineWorkCoreTestBase : ServiceScopedTestBase
    {
        /// <summary> 用于测试的假手机号. </summary>
        protected const String FakePhoneNumber = "11111111111";

        protected const String AppKey = "229d1cb589bb0ccd4bd1ec77";
        protected const String MasterSecret="b79a9e1fea89e45813e6ccfb";

        protected override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddFileManager(ConfigurationManager.ConnectionStrings["FineWorkAzureStorage"].ConnectionString)
                .AddDbSession(ConfigurationManager.ConnectionStrings["FineWork"].ConnectionString)
                .AddSessionProvider()
                .AddJPushClient(AppKey,MasterSecret)
                .AddFineWorkCoreServices();
        }
    }
}