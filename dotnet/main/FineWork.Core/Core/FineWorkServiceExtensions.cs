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
using Microsoft.Extensions.Logging;


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

        public static IServiceCollection AddImClient(this IServiceCollection serviceCollention, string id,string key,string master)
        {
            Args.NotNull(serviceCollention, nameof(serviceCollention));
            serviceCollention.AddScoped<IIMService>(services => new LCIMService(id,key,master));
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
                    new LazyResolver<IAnnouncementManager>(s),
                    new LazyResolver<ITaskTempManager>(s), 
                    s.GetService<IConfiguration>(),
                    s.GetRequiredService<IConversationManager>(),
                    s.GetRequiredService<IMemberManager>()));

            serviceCollection.AddScoped<ITaskTempManager>(
                s=>new TaskTempManager(
                     s.ResolveSessionProvider(),
                     s.GetRequiredService<ITaskManager>(),
                     s.GetRequiredService<IStaffManager>())
                );

            serviceCollection.AddScoped<IPartakerManager>(
                s => new PartakerManager(
                    s.ResolveSessionProvider(),
                    s.GetRequiredService<IStaffManager>(),
                    s.GetRequiredService<ITaskManager>(),
                    new LazyResolver<ITaskAlarmManager>(s),
                    s.GetRequiredService<IIMService>(),
                    s.GetRequiredService<INotificationManager>(),
                    s.GetRequiredService<IConfiguration>(),
                    s.GetRequiredService<IMemberManager>()
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
                    s.GetService<IConfiguration>())
                );

            serviceCollection.AddScoped<INotificationManager>(
                s => new NotificationManager(s.ResolveSessionProvider(),
                    s.GetRequiredService<JPushClient>(),
                    s.GetRequiredService<IAccountManager>(),
                    s.GetRequiredService<IConfiguration>()));

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
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<ITaskManager>() 
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


            serviceCollection.AddScoped<ITaskNoteManager>(s => new TaskNoteManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IIMService>()
                ));

            serviceCollection.AddScoped<IVoteOptionManager>(s => new VoteOptioinManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskVoteManager>()
                ));

            serviceCollection.AddScoped<IVoteManager>(s => new VoteManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IVoteOptionManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetService<IConfiguration>(),
                s.GetRequiredService<ITaskVoteManager>()
                ));

            serviceCollection.AddScoped<ITaskVoteManager>(s => new TaskVoteManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                new LazyResolver<IVoteManager>(s),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<IConfiguration>(),
                s.GetRequiredService<IPushLogManager>(),
                s.GetRequiredService<INotificationManager>()
                ));

            serviceCollection.AddScoped<IVotingManager>(s => new VotingManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IVoteOptionManager>(),
                s.GetRequiredService<ITaskVoteManager>()
                ));

            serviceCollection.AddScoped<ITaskAlarmManager>(s => new TaskAlarmManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<INotificationManager>(),
                s.GetService<IConfiguration>(),
                s.GetRequiredService<IConversationManager>(),
                s.GetRequiredService<IMemberManager>()));

            serviceCollection.AddScoped<IMemberManager>(s => new MemberManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IConversationManager>() 
                ));

            serviceCollection.AddScoped<IConversationManager>(s => new ConversationManager(
                s.ResolveSessionProvider(),
                new LazyResolver<ITaskManager>(s),
                new LazyResolver<ITaskAlarmManager>(s)));

            serviceCollection.AddScoped<IAlarmManager>(s => new AlarmManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<ITaskAlarmManager>(),
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<IAlarmTempManager>(),
                new LazyResolver<IPushLogManager>(s)
                ));

            serviceCollection.AddScoped<IAlarmTempManager>(s => new AlarmTempManagerr(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IFileManager>()
                ));

            serviceCollection.AddScoped<ITaskSharingManager>(s => new TaskSharingManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IFileManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetService<IConfiguration>(),
                new LazyResolver<IAnncAttManager>(s) 
                ));

            serviceCollection.AddScoped<IInvCodeManager>(s => new InvCodeManager(s.ResolveSessionProvider()));

            serviceCollection.AddScoped<IMomentManager>(s => new MomentManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IConfiguration>(),
                s.GetRequiredService<INotificationManager>(),
                s.GetRequiredService<IAccessTimeManager>(),
                new LazyResolver<IMomentCommentManager>(s),
                new LazyResolver<IMomentLikeManager>(s)
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
                s.GetRequiredService<ITaskSharingManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<IConfiguration>()
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
                s.GetRequiredService<IAnncAttManager>(), 
                s.GetRequiredService<IPartakerManager>(),
                s.GetRequiredService<IIMService>(),
                s.GetRequiredService<ITaskLogManager>(),
                s.GetRequiredService<IConfiguration>(),
                s.GetRequiredService<IAnncAlarmManager>(),
                s.GetRequiredService<IAnncAlarmRecManager>(),
                new LazyResolver<IAnncExecutorManager>(s),
                new LazyResolver<IAnncUpdateManager>(s),
                new LazyResolver<ITaskSharingManager>(s),
                new LazyResolver<IPushLogManager>(s)));

            serviceCollection.AddScoped<IAnncExecutorManager>(s => new AnncExecutorManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IAnnouncementManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IPartakerManager>()
                ));

            serviceCollection.AddScoped<IAnncAttManager>(s => new AnncAttManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnnouncementManager>(s),
                s.GetRequiredService<ITaskSharingManager>())); 

            serviceCollection.AddScoped<IAnncReviewManager>(s => new AnncReviewManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnnouncementManager>(s), 
                s.GetRequiredService<IIncentiveManager>()));

            serviceCollection.AddScoped<IAnncAlarmManager>(s => new AnncAlarmManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnnouncementManager>(s),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IAnncAlarmRecManager>() 
                ));

            serviceCollection.AddScoped<IAnncAlarmRecManager>(s => new AnncAlarmRecManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IAnncAlarmManager>(s),
                s.GetRequiredService<IStaffManager>()
                ));

            serviceCollection.AddScoped<IForumSectionManager>(s => new ForumSectionManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IAccessTimeManager>(),
                new LazyResolver<IForumTopicManager>(s),
                new LazyResolver<IForumCommentLikeManager>(s),
                new LazyResolver<IForumLikeManager>(s),
                new LazyResolver<IForumCommentManager>(s)));

            serviceCollection.AddScoped<IForumTopicManager>(s => new ForumTopicManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IForumSectionManager>(),
                s.GetRequiredService<IStaffManager>()));

            serviceCollection.AddScoped<IForumLikeManager>(s => new ForumLikeManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IForumTopicManager>(),
                s.GetRequiredService<IStaffManager>()));

            serviceCollection.AddScoped<IForumCommentManager>(s => new ForumCommentManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IForumTopicManager>(),
                s.GetRequiredService<IStaffManager>()));

            serviceCollection.AddScoped<IForumVoteManager>(s => new ForumVoteManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IForumTopicManager>(),
                s.GetRequiredService<IVoteManager>()
                ));

            serviceCollection.AddScoped<IForumCommentLikeManager>(s => new ForumCommentLikeManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IForumCommentManager>(),
                s.GetRequiredService<IStaffManager>()));

            serviceCollection.AddScoped<IForumSectionViewTimeManager>(s => new ForumSectionViewTimeManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IForumSectionManager>()));

            serviceCollection.AddScoped<IPlanManager>(s => new PlanManager(
                s.ResolveSessionProvider(),
                new LazyResolver<IPlanAlarmManager>(s),
                new LazyResolver<IPlanExecutorManager>(s),
                new LazyResolver<IPlanAtManager>(s), 
                new LazyResolver<IPushLogManager>(s), 
                s.GetRequiredService<IStaffManager>()
                ));

            serviceCollection.AddScoped<IPlanAlarmManager>(s => new PlanAlarmManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IPlanManager>()));

            serviceCollection.AddScoped<IPlanAtManager>(s => new PlanAtManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IPlanManager>(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<INotificationManager>(),
                s.GetRequiredService<IConfiguration>()
               ));

            serviceCollection.AddScoped<IPlanExecutorManager>(s => new PlanExecutorManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IPlanManager>(),
                s.GetRequiredService<IStaffManager>()
                ));

            serviceCollection.AddScoped<IAnncUpdateManager>(s => new AnncUpdateManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IStaffManager>(),
                s.GetRequiredService<IAnnouncementManager>()
                )); 

            serviceCollection.AddScoped<IPushLogManager>(s => new PushLogManager(
                s.ResolveSessionProvider(),
                s.GetRequiredService<IAnnouncementManager>(),
                s.GetRequiredService<IPlanManager>(),
                s.GetRequiredService<IAlarmManager>(),
                s.GetRequiredService<ITaskManager>(),
                s.GetRequiredService<IStaffManager>()
                ));

            return serviceCollection;
        }
    }
}
