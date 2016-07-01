using System;
using AppBoot.Common;
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
using cn.jpush.api;
using FineWork.Avatar;
using FineWork.Azure;  
using FineWork.Common;
using FineWork.Files;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;


namespace FineWork.Core
{
    public static class FineWorkServiceExtensions
    {
        private const String m_AvatarFileManagerFactoryName = "avatar";
        private const String m_OrgManagerFactoryName = "org";
        private const String m_StaffManagerFactoryName = "staff";
        private const String m_AlarmManagerFactoryName = "alarm";
        private const String m_TaskIncentiveManagerFactoryName = "taskIncentive";
        private const String m_PartakerManagerFactoryName = "partaker";

        public static FineWorkServiceBuilder WithFineWork(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            return new FineWorkServiceBuilder(serviceCollection);
        }

        public static FineWorkServiceBuilder AddCoreServices(this FineWorkServiceBuilder serviceBuilder)
        {
            if (serviceBuilder == null) throw new ArgumentNullException(nameof(serviceBuilder));
            AddFineWorkCoreServices(serviceBuilder.ServiceCollection);
            return serviceBuilder;
        }

        public static IServiceCollection AddDbSession(this IServiceCollection serviceCollection, String connectionString)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddScoped<FineWorkDbContext>(services => new FineWorkDbContext(connectionString, true));
            return serviceCollection;

        }

        public static IServiceCollection AddJPushClient(this IServiceCollection serviceCollection, String appKey,
            String masterSecret)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddScoped<JPushClient>(services => new JPushClient(appKey, masterSecret));
            return serviceCollection;
        }

        public static IServiceCollection AddImClient(this IServiceCollection serviceCollention, IConfiguration config)
        {
            Args.NotNull(serviceCollention, nameof(serviceCollention));
            serviceCollention.AddScoped<IIMService>(services => new LCIMService(config));
            return serviceCollention;
        }

        public static IServiceCollection AddSmsService(this IServiceCollection serviceCollention, string appId,
            string appKey)
        {
            Args.NotNull(serviceCollention, nameof(serviceCollention));
            serviceCollention.AddScoped<ISmsService>(services => new LCSmsService(appId, appKey));
            return serviceCollention;
        }


        public static IServiceCollection AddFileManager(this IServiceCollection serviceCollection,
            String connectionString)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            //Avatars are stored in the "public" container.
            serviceCollection.AddScoped<IFileManager>(
                services => new AzureFileManager(m_AvatarFileManagerFactoryName, connectionString, "public"));
            return serviceCollection;
        }

        public static IServiceCollection AddSessionProvider(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            //serviceCollection.AddScoped<ISessionProvider<AefSession>>(
            //    services => new AmbientSessionProvider<AefSession>(m_AmbientKey));
            serviceCollection.AddScoped<ISessionProvider<AefSession>>(
                services => new ResolvableSessionProvider<FineWorkDbContext>(services));

            return serviceCollection;
        }

        public static ISessionProvider<AefSession> ResolveSessionProvider(this IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.GetRequiredService<ISessionProvider<AefSession>>();
        }

        public static IServiceCollection AddFineWorkCoreServices(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddScoped<IPasswordService>(
                s => new PasswordService());
            serviceCollection.AddScoped<IEmailService>(
                s => new NullEmailService());
            serviceCollection.AddScoped<ISmsService>(
                s => new NullSmsService());

            serviceCollection.AddScoped<IAvatarManager>(
                s => new AvatarManager(s.GetRequiredNamedService<IFileManager>(m_AvatarFileManagerFactoryName)));

            serviceCollection.AddScoped<ISettingRepository>(
                s => new SettingRepository(s.ResolveSessionProvider()));
            serviceCollection.AddScoped<IAccountRepository>(
                s => new AccountRepository(s.ResolveSessionProvider()));
            serviceCollection.AddScoped<ILoginRepository>(
                s => new LoginRepository(s.ResolveSessionProvider()));
            serviceCollection.AddScoped<IRoleRepository>(
                s => new RoleRepository(s.ResolveSessionProvider()));
            serviceCollection.AddScoped<IClaimRepository>(
                s => new ClaimRepository(s.ResolveSessionProvider()));

            serviceCollection.AddScoped<ISettingManager>(
                s => new SettingManager(
                    s.GetRequiredService<ISettingRepository>()));
            serviceCollection.AddScoped<IAccountManager>(
                s => new AccountManager(
                    s.GetRequiredService<IAccountRepository>(),
                    s.GetRequiredService<ILoginRepository>(),
                    s.GetRequiredService<ISettingManager>(),
                    s.GetRequiredService<IPasswordService>(),
                    s.GetRequiredService<IAvatarManager>()));
            serviceCollection.AddScoped<IRoleManager>(
                s => new RoleManager(
                    s.GetRequiredService<IRoleRepository>(),
                    s.GetRequiredService<IAccountRepository>()));


            serviceCollection.AddScoped<IOrgManager>(
                s => new OrgManager(
                    s.ResolveSessionProvider(),
                    new LazyResolver<IStaffManager>(s),
                    s.GetRequiredService<IAvatarManager>()));


            serviceCollection.AddScoped<IStaffManager>(
                s => new StaffManager(
                    s.ResolveSessionProvider(),
                    s.GetRequiredService<IAccountManager>(),
                    s.GetRequiredService<IOrgManager>()));


            serviceCollection.AddScoped<ITaskManager>(
                s => new TaskManager(
                    s.ResolveSessionProvider(),
                    new LazyResolver<IOrgManager>(s),
                    new LazyResolver<IStaffManager>(s),
                    s.GetRequiredService<IIMService>(),
                    new LazyResolver<ITaskIncentiveManager>(s),
                    new LazyResolver<IAlarmManager>(s),
                    new LazyResolver<IPartakerManager>(s),
                    new LazyResolver<IPartakerInvManager>(s),
                    s.GetService<IConfiguration>()));

            serviceCollection.AddScoped<IPartakerManager>(
                s => new PartakerManager(
                    s.ResolveSessionProvider(),
                    s.GetRequiredService<IStaffManager>(),
                    s.GetRequiredService<ITaskManager>(),
                    new LazyResolver<ITaskAlarmManager>(s),
                    s.GetRequiredService<IIMService>(),
                    s.GetRequiredService<INotificationManager>(),
                    s.GetRequiredService<IConfiguration>()
                    ));

            serviceCollection.AddScoped<IStaffReqManager>(
                s => new StaffReqManager(s.ResolveSessionProvider(),
                    s.GetRequiredService<IAccountManager>(),
                    s.GetRequiredService<IOrgManager>(),
                    s.GetRequiredService<IStaffManager>())
                );

            serviceCollection.AddScoped<IStaffInvManager>(
                s => new StaffInvManager(s.ResolveSessionProvider(),
                    s.GetRequiredService<IAccountManager>(),
                    s.GetRequiredService<IOrgManager>(),
                    s.GetRequiredService<IStaffManager>(),
                    s.GetRequiredService<ISmsService>())
                );


            serviceCollection.AddScoped<IPartakerReqManager>(
                s => new PartakerReqManager(s.ResolveSessionProvider(),
                    new LazyResolver<IPartakerInvManager>(s),
                    s.GetRequiredService<IPartakerManager>(),
                    s.GetRequiredService<IIMService>())
                );

            serviceCollection.AddScoped<IPartakerInvManager>(
                s => new PartakerInvManager(s.ResolveSessionProvider(),
                    new LazyResolver<IPartakerManager>(s),
                    new LazyResolver<IPartakerReqManager>(s),
                    s.GetRequiredService<IIMService>(),
                    s.GetRequiredService<INotificationManager>(),
                    s.GetService<IConfiguration>())
                );

            serviceCollection.AddScoped<INotificationManager>(
                s => new NotificationManager(s.ResolveSessionProvider(),
                    s.GetRequiredService<JPushClient>(),
                    s.GetRequiredService<IAccountManager>()));

            serviceCollection.AddScoped<ITaskLogManager>(
                s => new TaskLogManager(s.ResolveSessionProvider(),
                    s.GetRequiredService<ITaskManager>(),
                    s.GetRequiredService<IStaffManager>())
                );

            serviceCollection.AddScoped<IIncentiveKindManager>(
                s => new IncentiveKindManager(s.ResolveSessionProvider()));

            serviceCollection.AddScoped<ITaskIncentiveManager>(s => new TaskIncentiveManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IIncentiveKindManager>()));

            serviceCollection.AddScoped<IIncentiveManager>(s => new IncentiveManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<ITaskIncentiveManager>(),
                s.GetRequiredService<ITaskLogManager>()
                ));

            serviceCollection.AddScoped<ITaskNewsManager>(s => new TaskNewsManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetService<IConfiguration>(),
                s.GetRequiredService<IAccessTimeManager>()
                ));


            serviceCollection.AddScoped<ITaskAnnouncementManager>(s => new TaskAnnouncementManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IIMService>()
                ));

            serviceCollection.AddScoped<IVoteOptionManager>(s => new VoteOptioinManager(
                s.ResolveSessionProvider()
                ));

            serviceCollection.AddScoped<IVoteManager>(s => new VoteManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IVoteOptionManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetService<IConfiguration>()
                ));

            serviceCollection.AddScoped<IVotingManager>(s => new VotingManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IVoteOptionManager>()
                ));

            serviceCollection.AddScoped<ITaskAlarmManager>(s => new TaskAlarmManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<INotificationManager>(),
                s.GetService<IConfiguration>()));


            serviceCollection.AddScoped<IAlarmManager>(s => new AlarmManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<ITaskAlarmManager>()
                )); 

            serviceCollection.AddScoped<ITaskSharingManager>(s => new TaskSharingManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IFileManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetService<IConfiguration>()
                ));

            serviceCollection.AddScoped<IInvCodeManager>(s => new InvCodeManager(s.ResolveSessionProvider()));

            serviceCollection.AddScoped<IMomentManager>(s => new MomentManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>()
                ));

            serviceCollection.AddScoped<IMomentFileManager>(s => new MomentFileManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IMomentManager>(),
                s.GetRequiredService<IFileManager>()
                ));

            serviceCollection.AddScoped<IMomentLikeManager>(s => new MomentLikeManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IMomentManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IConfiguration>(),
                s.GetRequiredService<INotificationManager>()));

            serviceCollection.AddScoped<IMomentCommentManager>(s => new MomentCommentManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IMomentManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IConfiguration>(),
                s.GetRequiredService<INotificationManager>()));

            serviceCollection.AddScoped<IAccessTimeManager>(s => new AccessTimeManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>()));

            serviceCollection.AddScoped<ITaskReportManager>(s => new TaskReportManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskReportAttManager>(),
                s.GetRequiredService<ITaskSharingManager>()
                ));

            serviceCollection.AddScoped<ITaskReportAttManager>(s => new TaskReportAttManager(
                s.ResolveSessionProvider(),
                 new LazyResolver<ITaskReportManager>(s),
                s.GetRequiredService<ITaskSharingManager>()
                ));

            serviceCollection.AddScoped<IAnnouncementManager>(s => new AnnouncementManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IAnncIncentiveManager>(),
                s.GetRequiredService<IAnncAttManager>(),
                s.GetRequiredService<ITaskSharingManager>(),
                s.GetRequiredService<IIncentiveManager>()));

            serviceCollection.AddScoped<IAnncAttManager>(s => new AnncAttManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnnouncementManager>(s),
                s.GetRequiredService<ITaskSharingManager>()));


            serviceCollection.AddScoped<IAnncIncentiveManager>(s => new AnncIncentiveManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnnouncementManager>(s),
                s.GetRequiredService<ITaskIncentiveManager>(),
                s.GetRequiredService<IIncentiveKindManager>(),
                s.GetRequiredService<IIncentiveManager>()
                ));
            return serviceCollection;
        }
    }
}
