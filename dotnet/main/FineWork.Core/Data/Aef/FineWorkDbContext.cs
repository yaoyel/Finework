using System;
using System.Data.Entity;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Logging;
using FineWork.Message;
using FineWork.Security.Repos.Aef;
using FineWork.Settings.Repos.Aef;
using Microsoft.Extensions.Logging;

namespace FineWork.Data.Aef
{
    /// <summary>
    /// Represents the FineWork <see cref="DbContext"/>.
    /// </summary>
    public class FineWorkDbContext : AefSession
    {
        private const String m_SqlDateTime2 = "datetime2";

        private const string m_NvarcharMax = "nvarchar(max)";

        public FineWorkDbContext()
            : this(LogManager.Factory)
        {
        }

        public FineWorkDbContext(String nameOrConnectionString)
            : this(nameOrConnectionString, true, LogManager.Factory)
        {
        }

        public FineWorkDbContext(ILoggerFactory loggerFactory)
            : this("FineWork", true, loggerFactory)
        {
        }

        public FineWorkDbContext(String nameOrConnectionString, bool writeChangesImmediately)
            : this(nameOrConnectionString, writeChangesImmediately, LogManager.Factory)
        {
        }

        public FineWorkDbContext(String nameOrConnectionString, bool writeChangesImmediately,
            ILoggerFactory loggerFactory)
            : base(nameOrConnectionString, writeChangesImmediately)
        {
            if (loggerFactory == null) throw new ArgumentNullException("loggerFactory");
            this.Logger = loggerFactory.CreateLogger<FineWorkDbContext>();
            this.Database.Log = this.Logger.LogInformation;
        }


        private ILogger Logger { get; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("finework");

            var settingItemConfig = modelBuilder.Entity<SettingEntity>()
                .ToTable("fw_Settings")
                .HasKey(settingItem => settingItem.Id);

            var accountConfig = modelBuilder.Entity<AccountEntity>()
                .ToTable("fw_Accounts")
                .HasKey(account => account.Id);
            accountConfig.Property(x => x.CreatedAt).HasColumnType(m_SqlDateTime2);
            accountConfig.Property(x => x.LockEndAt).HasColumnType(m_SqlDateTime2);

            //fw_Roles
            var roleConfig = modelBuilder.Entity<RoleEntity>()
                .ToTable("fw_Roles")
                .HasKey(role => role.Id);

            //fw_AccountRoles
            accountConfig.HasMany(account => account.Roles)
                .WithMany(role => role.Accounts)
                .Map(
                    accountRoles =>
                        accountRoles.ToTable("fw_AccountRoles").MapLeftKey("AccountId").MapRightKey("RoleId"));

            //fw_Logins
            var loginConfig = modelBuilder.Entity<LoginEntity>()
                .ToTable("fw_Logins")
                .HasKey(login => login.Id);
            loginConfig.HasRequired(login => login.Account)
                .WithMany(account => account.Logins)
                .Map(fk => fk.MapKey("AccountId"));

            var claimConfig = modelBuilder.Entity<ClaimEntity>()
                .ToTable("fw_Claims")
                .HasKey(claim => claim.Id);
            claimConfig.HasRequired(claim => claim.Account)
                .WithMany(account => account.Claims)
                .Map(fk => fk.MapKey("AccountId"));

            var orgConfig = modelBuilder.Entity<OrgEntity>()
                .ToTable("fw_Orgs")
                .HasKey(org => org.Id);
            orgConfig.HasOptional(org => org.AdminStaff)
                .WithOptionalDependent().Map(fk => fk.MapKey("AdminStaffId"));


            var staffConfig = modelBuilder.Entity<StaffEntity>()
                .ToTable("fw_Staffs")
                .HasKey(staff => staff.Id);
            staffConfig.HasRequired(staff => staff.Account)
                .WithMany()
                .Map(fk => fk.MapKey("AccountId"));
            staffConfig.HasRequired(staff => staff.Org)
                .WithMany(org => org.Staffs)
                .Map(fk => fk.MapKey("OrgId"));

            var taskConfig = modelBuilder.Entity<TaskEntity>()
                .ToTable("fw_Tasks")
                .HasKey(task => task.Id);
            taskConfig.HasOptional(task => task.Creator)
                .WithMany()
                .Map(fk => fk.MapKey("CreatorStaffId"));
            taskConfig.HasOptional(task => task.ParentTask)
                .WithMany(task => task.ChildTasks)
                .Map(fk => fk.MapKey("ParentTaskId"));

            taskConfig.HasRequired(p => p.Conversation)
                .WithMany()
                .HasForeignKey(fk => fk.ConversationId)
                .WillCascadeOnDelete();

            taskConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var partakerConfig = modelBuilder.Entity<PartakerEntity>()
                .ToTable("fw_Partakers")
                .HasKey(partaker => partaker.Id);
            partakerConfig.HasRequired(partaker => partaker.Staff)
                .WithMany(staff => staff.Partakers)
                .Map(fk => fk.MapKey("StaffId"));
            partakerConfig.HasRequired(partaker => partaker.Task)
                .WithMany(task => task.Partakers)
                .Map(fk => fk.MapKey("TaskId"));

            var staffReqConfig = modelBuilder.Entity<StaffReqEntity>()
                .ToTable("fw_StaffReqs")
                .HasKey(req => req.Id);
            staffReqConfig.HasRequired(req => req.Org)
                .WithMany(org => org.StaffReqs)
                .Map(fk => fk.MapKey("OrgId"));
            staffReqConfig.HasRequired(req => req.Account)
                .WithMany().Map(fk => fk.MapKey("AccountId"));

            staffReqConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            staffReqConfig.Property(p => p.ReviewAt).HasColumnType(m_SqlDateTime2);

            var staffInvConfig = modelBuilder.Entity<StaffInvEntity>()
                .ToTable("fw_staffInvs")
                .HasKey(inv => inv.Id);
            staffInvConfig.HasRequired(req => req.Org)
                .WithMany(org => org.StaffInvs)
                .Map(fk => fk.MapKey("OrgId"));

            staffInvConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            staffInvConfig.Property(p => p.ReviewAt).HasColumnType(m_SqlDateTime2);

            var partakerInvConfig = modelBuilder.Entity<PartakerInvEntity>()
                .ToTable("fw_PartakerInvs")
                .HasKey(inv => inv.Id);
            partakerInvConfig.HasRequired(inv => inv.Task)
                .WithMany(task => task.PartakerInvs).Map(fk => fk.MapKey("TaskId"));
            partakerInvConfig.HasRequired(inv => inv.Staff)
                .WithMany(staff => staff.PartakerInvs).Map(fk => fk.MapKey("StaffId"));
            partakerInvConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            partakerInvConfig.Property(p => p.ReviewAt).HasColumnType(m_SqlDateTime2);

            var partakerReqConfig = modelBuilder.Entity<PartakerReqEntity>()
                .ToTable("fw_PartakerReqs")
                .HasKey(req => req.Id);
            partakerReqConfig.HasRequired(req => req.Task)
                .WithMany().Map(fk => fk.MapKey("TaskId"));
            partakerReqConfig.HasRequired(req => req.Staff)
                .WithMany().Map(fk => fk.MapKey("StaffId"));
            partakerReqConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            partakerReqConfig.Property(p => p.ReviewAt).HasColumnType(m_SqlDateTime2);

            var taskNewsConfig = modelBuilder.Entity<TaskNewsEntity>()
                .ToTable("fw_TaskNews")
                .HasKey(news => news.Id);
            taskNewsConfig.HasRequired(news => news.Task)
                .WithMany(task => task.Newses).Map(fk => fk.MapKey("TaskId"));
            taskNewsConfig.HasRequired(news => news.Staff)
                .WithMany(staff => staff.Newses).Map(fk => fk.MapKey("StaffId"));
            taskNewsConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskLogConfig = modelBuilder.Entity<TaskLogEntity>()
                .ToTable("fw_TaskLogs")
                .HasKey(log => log.Id);
            taskLogConfig.HasRequired(log => log.Task)
                .WithMany(task => task.Logs).Map(fk => fk.MapKey("TaskId"));
            taskLogConfig.HasRequired(log => log.Staff)
                .WithMany(staff => staff.Logs).Map(fk => fk.MapKey("StaffId"));
            taskLogConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskAlarmConfig = modelBuilder.Entity<TaskAlarmEntity>()
                .ToTable("fw_TaskAlarms")
                .HasKey(alarm => alarm.Id);
            taskAlarmConfig.HasRequired(alarm => alarm.Task)
                .WithMany(task => task.Alarms).Map(fk => fk.MapKey("TaskId"));
            taskAlarmConfig.HasRequired(alarm => alarm.Staff)
                .WithMany(staff => staff.Alarms).Map(fk => fk.MapKey("StaffId"));
            taskAlarmConfig.HasRequired(p => p.Conversation)
                .WithMany(p => p.TaskAlarms)
                .Map(fk => fk.MapKey("ConversationId"));

            taskAlarmConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
 


            var deviceRegConfig = modelBuilder.Entity<DeviceRegistrationEntity>()
                .ToTable("fw_DeviceRegistrations")
                .HasKey(device => device.Id);
            deviceRegConfig.HasRequired(req => req.Account)
                .WithMany().Map(fk => fk.MapKey("AccountId"));

            modelBuilder.Entity<IncentiveKindEntity>()
                .ToTable("fw_IncentiveKinds")
                .HasKey(kind => kind.Id);

            var taskIncentiveConfig = modelBuilder.Entity<TaskIncentiveEntity>()
                .ToTable("fw_TaskIncentives")
                .HasKey(takIncentive => takIncentive.Id);

            taskIncentiveConfig.HasRequired(takIncentive => takIncentive.Task)
                .WithMany(task => task.Incentives)
                .Map(fk => fk.MapKey("TaskId"));


            taskIncentiveConfig.HasRequired(takIncentive => takIncentive.IncentiveKind)
                .WithMany(kind => kind.TaskIncentives)
                .Map(fk => fk.MapKey("IncentiveKindId"));
            taskIncentiveConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var incentiveConfig = modelBuilder.Entity<IncentiveEntity>()
                .ToTable("fw_Incentives")
                .HasKey(incentive => incentive.Id);

            incentiveConfig.HasRequired(p => p.TaskIncentive)
                .WithMany(taskIncentive => taskIncentive.Incentives)
                .Map(fk => fk.MapKey("TaskIncentiveId"));

            incentiveConfig.HasRequired(p => p.SenderStaff)
                .WithMany(staff => staff.SentIncentives)
                .Map(fk => fk.MapKey("SenderStaffId"));

            incentiveConfig.HasRequired(p => p.ReceiverStaff)
                .WithMany(staff => staff.ReceivedIncentives)
                .Map(fk => fk.MapKey("ReceiverStaffId"));

            incentiveConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            incentiveConfig.Property(p => p.Quantity).HasPrecision(10, 2);

            var alarmPeriodConfig = modelBuilder.Entity<AlarmEntity>()
                .ToTable("fw_Alarms")
                .HasKey(p => p.Id);

            alarmPeriodConfig.HasRequired(p => p.Task)
                .WithMany(task => task.AlarmPeriods)
                .Map(fk => fk.MapKey("TaskId"));

            alarmPeriodConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            //var alarmPeriodTempConfig = modelBuilder.Entity<AlarmTempEntity>()
            //    .ToTable("fw_AlarmTemps")
            //    .HasKey(p => p.Id);

            //alarmPeriodTempConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var voteCofnig = modelBuilder.Entity<VoteEntity>()
                .ToTable("fw_Votes")
                .HasKey(p => p.Id);

            voteCofnig.HasRequired(vote => vote.Creator)
                .WithMany(creator => creator.Votes)
                .Map(fk => fk.MapKey("StaffId"));

            voteCofnig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            voteCofnig.Property(p => p.StartAt).HasColumnType(m_SqlDateTime2);
            voteCofnig.Property(p => p.EndAt).HasColumnType(m_SqlDateTime2);

            var voteOptionConfig = modelBuilder.Entity<VoteOptionEntity>()
                .ToTable("fw_VoteOptions")
                .HasKey(pk => pk.Id);

            voteOptionConfig.HasRequired(p => p.Vote)
                .WithMany(vote => vote.VoteOptions)
                .Map(fk => fk.MapKey("VoteId"));

            voteOptionConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var votingConfig = modelBuilder.Entity<VotingEntity>()
                .ToTable("fw_Votings")
                .HasKey(pk => pk.Id);

            votingConfig.HasRequired(p => p.Option)
                .WithMany(option => option.Votings)
                .Map(fk => fk.MapKey("VoteOptionId"));
            votingConfig.HasRequired(p => p.Staff)
                .WithMany(staff => staff.Votings)
                .Map(fk => fk.MapKey("StaffId"));

            votingConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskTransferConfig = modelBuilder.Entity<TaskTransferEntity>()
                .ToTable("fw_TaskTransfers")
                .HasKey(pk => pk.Id);

            taskTransferConfig.HasRequired(transfer => transfer.Staff)
                .WithMany(staff => staff.TaskTransfers)
                .Map(fk => fk.MapKey("StaffId"));

            taskTransferConfig.HasRequired(transfer => transfer.AttachedTask)
                .WithMany(task => task.AttachedTaskTransfers)
                .Map(fk => fk.MapKey("AttachedTaskId"));

            taskTransferConfig.HasRequired(transfer => transfer.Task)
                .WithMany(task => task.DetachedTaskTransfers)
                .Map(fk => fk.MapKey("TaskId"));

            taskTransferConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            taskTransferConfig.Property(p => p.AttachedAt).HasColumnType(m_SqlDateTime2);
            taskTransferConfig.Property(p => p.DetachedAt).HasColumnType(m_SqlDateTime2);

            var taskSharingConfig = modelBuilder.Entity<TaskSharingEntity>()
                .ToTable("fw_TaskSharings");

            taskSharingConfig.HasRequired(sharing => sharing.Staff)
                .WithMany(staff => staff.TaskSharings)
                .Map(fk => fk.MapKey("StaffId"));

            taskSharingConfig.HasRequired(sharing => sharing.Task)
                .WithMany(task => task.TaskSharings)
                .Map(fk => fk.MapKey("TaskId"));
            taskSharingConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var invCode = modelBuilder.Entity<InvCodeEntity>()
                .ToTable("fw_InvCodes");

            invCode.HasOptional(t => t.Org).WithMany()
                .Map(fk => fk.MapKey("OrgId"));

            invCode.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            invCode.Property(p => p.ExpiredAt).HasColumnType(m_SqlDateTime2);


            var mementConfig = modelBuilder.Entity<MomentEntity>()
                .ToTable("fw_Moments")
                .HasKey(p => p.Id);

            mementConfig.HasRequired(mement => mement.Staff)
                .WithMany(staff => staff.Moments)
                .Map(fk => fk.MapKey("StaffId"));

            mementConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var mementFileConfig = modelBuilder.Entity<MomentFileEntity>()
                .ToTable("fw_MomentFiles")
                .HasKey(p => p.Id);

            mementFileConfig.HasRequired(file => file.Moment)
                .WithMany(mement => mement.MomentFiles)
                .Map(fk => fk.MapKey("MomentId"));

            mementFileConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var mementLikeConfig = modelBuilder.Entity<MomentLikeEntity>()
                .ToTable("fw_MomentLikes")
                .HasKey(p => p.Id);

            mementLikeConfig.HasRequired(like => like.Moment)
                .WithMany(mement => mement.MomentLikes)
                .Map(fk => fk.MapKey("MomentId"));

            mementLikeConfig.HasRequired(like => like.Staff)
                .WithMany(staff => staff.MomentLikes)
                .Map(fk => fk.MapKey("StaffId"));

            mementLikeConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var mementCommentConfig = modelBuilder.Entity<MomentCommentEntity>()
                .ToTable("fw_MomentComments")
                .HasKey(p => p.Id);

            mementCommentConfig.HasRequired(comment => comment.Staff)
                .WithMany(staff => staff.MomentComments)
                .Map(fk => fk.MapKey("StaffId"));

            mementCommentConfig.HasRequired(comment => comment.Moment)
                .WithMany(mement => mement.MomentComments)
                .Map(fk => fk.MapKey("MomentId"));

            mementCommentConfig.HasOptional(comment => comment.TargetComment)
                .WithMany(comment => comment.DerivativeComments)
                .Map(fk => fk.MapKey("TargetCommentId"));

            mementCommentConfig.HasOptional(comment => comment.ToStaff)
                .WithMany()
                .Map(fk => fk.MapKey("ToStaffId"));

            mementCommentConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var accessTimeConfig = modelBuilder.Entity<AccessTimeEntity>()
                .ToTable("fw_AccessTimes")
                .HasKey(p => p.Id);

            //?
            accessTimeConfig.HasRequired(p => p.Staff)
                .WithMany(p => p.AccessTime)
                .HasForeignKey(fk => fk.StaffId)
                .WillCascadeOnDelete();

            accessTimeConfig.Property(p => p.LastEnterOrgAt).HasColumnType(m_SqlDateTime2);
            accessTimeConfig.Property(p => p.LastViewCommentAt).HasColumnType(m_SqlDateTime2);
            accessTimeConfig.Property(p => p.LastViewMomentAt).HasColumnType(m_SqlDateTime2);
            accessTimeConfig.Property(p => p.LastViewNewsAt).HasColumnType(m_SqlDateTime2);
            accessTimeConfig.Property(p => p.LastViewForumAt).HasColumnType(m_SqlDateTime2);
            accessTimeConfig.Property(p => p.LastViewForumCommentAt).HasColumnType(m_SqlDateTime2);

            var taskAnnouncementConfig = modelBuilder.Entity<TaskNoteEntity>()
                .ToTable("fw_TaskNotes")
                .HasKey(announcement => announcement.Id);
            taskAnnouncementConfig.HasRequired(announcement => announcement.Task)
                .WithMany(task => task.Notes).Map(fk => fk.MapKey("TaskId"));
            taskAnnouncementConfig.HasRequired(announcement => announcement.Staff)
                .WithMany(staff => staff.Promise).Map(fk => fk.MapKey("StaffId"));
            taskAnnouncementConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskReportConfig = modelBuilder.Entity<TaskReportEntity>()
                .ToTable("fw_TaskReports")
                .HasKey(p => p.Id);

            taskReportConfig.HasRequired(p => p.Task)
                .WithOptional(task => task.Report)
                .Map(fk => fk.MapKey("TaskId")).WillCascadeOnDelete();
            taskReportConfig.Property(p => p.EndedAt).HasColumnType(m_SqlDateTime2);
            taskReportConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskReportAttConfig = modelBuilder.Entity<TaskReportAttEntity>()
                .ToTable("fw_TaskReportAtts")
                .HasKey(p => p.Id);

            taskReportAttConfig.HasRequired(att => att.TaskSharing)
                .WithMany()
                .Map(fk => fk.MapKey("TaskSharingId")).WillCascadeOnDelete();

            taskReportAttConfig.HasRequired(att => att.TaskReport)
                .WithMany(report => report.Atts)
                .Map(fk => fk.MapKey("TaskReportId"));

            taskReportAttConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var anncConfig = modelBuilder.Entity<AnnouncementEntity>()
                .ToTable("fw_Announcements")
                .HasKey(p => p.Id);

            anncConfig.HasRequired(annc => annc.Task)
                .WithMany(task => task.Announcements)
                .Map(fk => fk.MapKey("TaskId"));

            anncConfig.HasRequired(annc => annc.Creator)
                .WithMany(staff => staff.Announcements)
                .Map(fk => fk.MapKey("CreatorId"));

            anncConfig.HasOptional(annc => annc.Inspecter)
                .WithMany()
                .Map(fk => fk.MapKey("InspecterId"));

            anncConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            anncConfig.Property(p => p.EndAt).HasColumnType(m_SqlDateTime2);
            anncConfig.Property(p => p.StartAt).HasColumnType(m_SqlDateTime2);

            var anncAttConfig = modelBuilder.Entity<AnncAttEntity>()
                .ToTable("fw_AnncAtts")
                .HasKey(p => p.Id);

            anncAttConfig.HasRequired(p => p.Announcement)
                .WithMany(annc => annc.Atts)
                .Map(fk => fk.MapKey("AnncId"));

            anncAttConfig.HasRequired(p => p.TaskSharing)
                .WithMany()
                .Map(p => p.MapKey("TaskSharingId"))
                .WillCascadeOnDelete();

            anncAttConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var anncReviewConfig = modelBuilder.Entity<AnncReviewEntity>()
                .ToTable("fw_AnncReviews")
                .HasKey(p => p.Id);

            anncReviewConfig.HasRequired(p => p.Annc)
                .WithMany(annc => annc.Reviews)
                .Map(fk => fk.MapKey("AnncId"));

            anncReviewConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            anncReviewConfig.Property(p => p.DelayAt).HasColumnType(m_SqlDateTime2);

            var taskVoteConfig = modelBuilder.Entity<TaskVoteEntity>()
                .ToTable("fw_TaskVotes")
                .HasKey(p => p.Id);

            taskVoteConfig.HasRequired(p => p.Task)
                .WithMany(task => task.TaskVotes)
                .Map(fk => fk.MapKey("TaskId"));

            taskVoteConfig.HasRequired(p => p.Vote)
                .WithMany()
                .Map(fk => fk.MapKey("VoteId")).WillCascadeOnDelete();

            var forumSectionConfig = modelBuilder.Entity<ForumSectionEntity>()
                .ToTable("fw_ForumSections")
                .HasKey(p => p.Id);

            forumSectionConfig.HasRequired(p => p.Staff)
                .WithMany(staff => staff.ForumSections)
                .Map(fk => fk.MapKey("StaffId"));

            forumSectionConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var forumTopicConfig = modelBuilder.Entity<ForumTopicEntity>()
                .ToTable("fw_ForumTopics")
                .HasKey(p => p.Id);

            forumTopicConfig.HasRequired(p => p.ForumSection)
                .WithMany(section => section.ForumTopics)
                .Map(fk => fk.MapKey("ForumSectionId"));

            forumTopicConfig.HasRequired(p => p.Staff)
                .WithMany(staff => staff.ForumTopics)
                .Map(fk => fk.MapKey("StaffId"));

            forumTopicConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var forumCommentConfig = modelBuilder.Entity<ForumCommentEntity>()
                .ToTable("fw_ForumComments")
                .HasKey(p => p.Id);

            forumCommentConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            forumCommentConfig.HasRequired(p => p.ForumTopic)
                .WithMany(topic => topic.ForumComments)
                .Map(fk => fk.MapKey("TopicId"));

            forumCommentConfig.HasOptional(comment => comment.TargetComment)
                .WithMany(comment => comment.DerivativeComments)
                .Map(fk => fk.MapKey("TargetCommentId"));


            forumCommentConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            forumCommentConfig.Property(p => p.LastUpdatedAt).HasColumnType(m_SqlDateTime2);

            var forumLikeConfig = modelBuilder.Entity<ForumLikeEntity>()
                .ToTable("fw_ForumLikes")
                .HasKey(p => p.Id);

            forumLikeConfig.HasRequired(p => p.ForumTopic)
                .WithMany(topic => topic.ForumLikes)
                .Map(fk => fk.MapKey("TopicId"));

            forumLikeConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));
            forumLikeConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var forumVoteConfig = modelBuilder.Entity<ForumVoteEntity>()
                .ToTable("fw_ForumVotes")
                .HasKey(p => p.Id);

            forumVoteConfig.HasRequired(p => p.ForumTopic)
                .WithOptional(topic => topic.ForumVote)
                .Map(fk => fk.MapKey("TopicId"));

            forumVoteConfig.HasRequired(p => p.Vote)
                .WithMany()
                .Map(fk => fk.MapKey("VoteId")).WillCascadeOnDelete();


            var commentLikeConfig = modelBuilder.Entity<ForumCommentLikeEntity>()
                .ToTable("fw_ForumCommentLikes")
                .HasKey(p => p.Id);

            commentLikeConfig.HasRequired(p => p.ForumComment)
                .WithMany(p => p.Likes)
                .Map(fk => fk.MapKey("ForumCommentId"));

            commentLikeConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            commentLikeConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var memberConfig = modelBuilder.Entity<MemberEntity>()
                .ToTable("fw_Members")
                .HasKey(p => p.Id);

            memberConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            memberConfig.HasRequired(p => p.Conversation)
                .WithMany(convr => convr.Members)
                .Map(fk => fk.MapKey("ConversationId"));

            memberConfig.Property(p => p.ClearLogAt).HasColumnType(m_SqlDateTime2);

            var convrConfig = modelBuilder.Entity<ConversationEntity>()
                .ToTable("fw_Conversations")
                .HasKey(p => p.Id);

            convrConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var anncAlarmConfig = modelBuilder.Entity<AnncAlarmEntity>()
                .ToTable("fw_AnncAlarms")
                .HasKey(p => p.Id);

            anncAlarmConfig.HasRequired(p => p.Annc)
                .WithMany(s => s.Alarms)
                .Map(fk => fk.MapKey("AnncId"));

            anncAlarmConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var anncExecutorConfig = modelBuilder.Entity<AnncExecutorEntity>()
                .ToTable("fw_AnncExecutors")
                .HasKey(p => p.Id);

            anncExecutorConfig.HasRequired(p => p.Annc)
                .WithMany(s => s.Executors)
                .Map(fk => fk.MapKey("AnncId"));

            anncExecutorConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            anncExecutorConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var anncAlarmRecConfig = modelBuilder.Entity<AnncAlarmRecEntity>()
                .ToTable("fw_anncAlarmRecs").HasKey(p => p.Id);

            anncAlarmRecConfig.HasRequired(p => p.AnncAlarm)
                .WithMany(s => s.Recs)
                .Map(fk => fk.MapKey("AnncAlarmId"));

            anncAlarmRecConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            anncAlarmRecConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var forumSectionViewTimeConfig = modelBuilder.Entity<ForumSectionViewEntity>()
                .ToTable("fw_ForumSectionViewTimes")
                .HasKey(p => p.Id);

            forumSectionViewTimeConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));


            forumSectionViewTimeConfig.HasRequired(p => p.ForumSection)
                .WithMany(p => p.ViewStaffs)
                .Map(fk => fk.MapKey("ForumSectionId"));

            forumSectionViewTimeConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);


            var planConfig = modelBuilder.Entity<PlanEntity>()
                .ToTable("fw_Plans")
                .HasKey(p => p.Id);

            planConfig.HasRequired(p => p.Creator)
                .WithMany()
                .Map(fk => fk.MapKey("CreatorId"));
            planConfig.HasOptional(p => p.Inspecter)
                .WithMany()
                .Map(fk => fk.MapKey("InspecterId"));

            planConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            planConfig.Property(p => p.StartAt).HasColumnType(m_SqlDateTime2);
            planConfig.Property(p => p.EndAt).HasColumnType(m_SqlDateTime2);

            var planExecutorConfig = modelBuilder.Entity<PlanExecutorEntity>()
                .ToTable("fw_PlanExecutors")
                .HasKey(p => p.Id);

            planExecutorConfig.HasRequired(p => p.Plan)
                .WithMany(plan => plan.Executors)
                .Map(fk => fk.MapKey("PlanId"));

            planExecutorConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            planExecutorConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var planAlarmConfig = modelBuilder.Entity<PlanAlarmEnitty>()
                .ToTable("fw_PlanAlarms")
                .HasKey(p => p.Id);

            planAlarmConfig.HasRequired(p => p.Plan)
                .WithMany(s => s.Alarms)
                .Map(fk => fk.MapKey("PlanId"));

            planAlarmConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            planAlarmConfig.Property(p => p.Time).HasColumnType(m_SqlDateTime2);

            var planAtConfig = modelBuilder.Entity<PlanAtEntity>()
                .ToTable("fw_PlanAts")
                .HasKey(p => p.Id);


            planAtConfig.HasRequired(p => p.Plan)
                .WithMany(plan => plan.Ats)
                .Map(fk => fk.MapKey("PlanId"));

            planAtConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            planAtConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var anncUpdate = modelBuilder.Entity<AnncUpdateEntity>()
                .ToTable("fw_AnncUpdates")
                .HasKey(p => p.Id);

            anncUpdate.HasRequired(p => p.Annc)
                .WithMany(annc => annc.Updates)
                .Map(fk => fk.MapKey("AnncId"));

            anncUpdate.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            anncUpdate.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);

            var taskTempConfig = modelBuilder.Entity<TaskTempEntity>()
                .ToTable("fw_TaskTemps")
                .HasKey(p => p.Id);

            taskTempConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));

            taskTempConfig.HasRequired(p => p.Task)
                 .WithMany()
                .Map(fk => fk.MapKey("TaskId"));

            taskTempConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
            taskTempConfig.Property(p => p.LastUpdatedAt).HasColumnType(m_SqlDateTime2);



            var pushLogConfig = modelBuilder.Entity<PushLogEntity>()
                .ToTable("fw_PushLogs")
                .HasKey(p => p.Id);

            pushLogConfig.HasRequired(p => p.Staff)
                .WithMany()
                .Map(fk => fk.MapKey("StaffId"));
            

            taskTempConfig.Property(p => p.CreatedAt).HasColumnType(m_SqlDateTime2);
        }
    }
}