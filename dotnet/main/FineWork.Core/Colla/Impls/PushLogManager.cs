using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class PushLogManager : AefEntityManager<PushLogEntity, Guid>, IPushLogManager
    {
        public PushLogManager(ISessionProvider<AefSession> sessionProvider,
            IAnnouncementManager anncManager,
            IPlanManager planManager,
            IAlarmManager alarmManager,
            ITaskManager taskManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(anncManager, nameof(anncManager));
            Args.NotNull(planManager, nameof(planManager));
            Args.NotNull(alarmManager, nameof(alarmManager));
            Args.NotNull(taskManager, nameof(taskManager));

            m_AnncManager = anncManager;
            m_PlanManager = planManager;
            m_AlarmManager = alarmManager;
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
        }

        private readonly IAnnouncementManager m_AnncManager;
        private readonly IPlanManager m_PlanManager;
        private readonly IAlarmManager m_AlarmManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;


        public PushLogEntity CreatePushLog(Guid staffId, Guid targetId, PushKinds pushKind)
        {
            DeleteByStaffAndTargetId(targetId, staffId);

            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;
            var pushLog = new PushLogEntity();
            pushLog.Id = Guid.NewGuid(); 
            pushLog.Staff = staff;
            pushLog.TargetId = targetId;
            pushLog.TargetType = pushKind;
            pushLog.CreatedAt = GetCurrentTimeForUtc_8();
            if (staff != null)
                this.InternalInsert(pushLog);

            return pushLog;
        }

        public PushLogEntity FindById(Guid pushLogId)
        {
            return this.InternalFind(pushLogId);
        }

        void DeleteByStaffAndTargetId(Guid targetId, Guid staffId)
        {
            var logs = this.InternalFetch(p => p.Staff.Id == staffId && p.TargetId == targetId);
            if(logs.Any())
                logs.ToList().ForEach(this.InternalDelete);
        }

        public void DeletePushLogs(params Guid[] pushLogIds)
        {
            if (pushLogIds.Length == 0) return;

            var pushLogs = this.InternalFetch(p => pushLogIds.Contains(p.Id)).ToList();

            if (pushLogs.Any())
                pushLogs.ForEach(InternalDelete);
        }

        public void DeletePushLogsByAlarm(Guid alarmId)
        {
            this.DeletePushLogsByTargetId(alarmId);
        }


        public void DeletePushLogsByAnnc(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnncManager, anncId).Annc;
            if (annc == null) return;

            var anncAlarms = annc.Alarms.ToList();

            if (anncAlarms.Any())
                anncAlarms.ForEach(p => DeletePushLogsByTargetId(p.Id));
        }

        public void DeletePushLogsByStaffId(Guid staffId, PushKinds[] pushKinds)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;

            if (staff == null) return;

            var pushLogs = this.InternalFetch(p => p.Staff.Id == staffId && pushKinds.Contains(p.TargetType)).ToList();

            if (pushLogs.Any())
                pushLogs.ForEach(InternalDelete);
        }

        public void ChanageViewedStatus(Guid pushLogId)
        {
            var pushLog = PushLogExistsResult.Check(this, pushLogId).ThrowIfFailed().PushLog;

            pushLog.IsViewed = true;
            this.InternalUpdate(pushLog);
        }

        public void DeletePushLogsByPlan(Guid planId)
        {
            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).Plan;
            if (plan == null) return;

            var planAlarms = plan.Alarms.ToList();

            if (planAlarms.Any())
                planAlarms.ForEach(p => DeletePushLogsByTargetId(p.Id));
        }

        public void DeletePushLogsByTargetId(Guid targetId)
        {
            var pushLogs = this.InternalFetch(p => p.TargetId == targetId).ToList();
            if (pushLogs.Any())
                pushLogs.ForEach(this.InternalDelete);
        }

        public IEnumerable<PushLogEntity> FetchPushLogsByStaffId(Guid staffId)
        {
            var dbContext = this.Session.DbContext;
            var alarmSet = dbContext.Set<AlarmEntity>();
            var voteSet = dbContext.Set<TaskVoteEntity>();
            var anncSet = dbContext.Set<AnnouncementEntity>();

            var alarms = this.InternalFetch(p => p.Staff.Id == staffId && p.TargetType == PushKinds.Alarm)
                .Join(alarmSet, u => u.TargetId, c => c.Id, (u, c) => new PushLogEntity()
                {
                    Id = u.Id,
                    TargetId = u.TargetId,
                    Content = c.Content,
                    CreatedAt = u.CreatedAt,
                    Task = c.Task,
                    TargetType = u.TargetType,
                    IsViewed = u.IsViewed,
                    ShortTime =c.ShortTime,
                    Repeat = c.Weekdays
                });


            var anncs = this.InternalFetch(p => p.Staff.Id == staffId && p.TargetType == PushKinds.Annc)
                .Join(anncSet, u => u.TargetId, c => c.Id, (u, c) => new PushLogEntity()
                {
                    Id = u.Id,
                    TargetId = c.Id,
                    Content = c.Content,
                    CreatedAt = u.CreatedAt,
                    Task = c.Task,
                    TargetType = u.TargetType,
                    IsViewed = u.IsViewed 
                });
             
            var votes = this.InternalFetch(p => p.Staff.Id == staffId && p.TargetType == PushKinds.Vote)
                .Join(voteSet, u => u.TargetId, c => c.Vote.Id, (u, c) => new PushLogEntity()
                {
                    Id = u.Id,
                    TargetId = u.TargetId,
                    Content = c.Vote.Subject,
                    CreatedAt = u.CreatedAt,
                    Task = c.Task,
                    TargetType = u.TargetType,
                    IsViewed = u.IsViewed 
                });

            return alarms.Union(anncs).Union(votes);
        }


        public IEnumerable<PushLogEntity> FetchPushLogsByKinds(Guid staffId,PushKinds[] pushKinds)
        {
            return this.FetchPushLogsByStaffId(staffId).Where(p => pushKinds.Contains(p.TargetType));
        }

        DateTime GetCurrentTimeForUtc_8()
        { 
            TimeZoneInfo local = TimeZoneInfo.Local;
            var time = DateTime.Now.AddHours(8 - local.BaseUtcOffset.Hours);

            var timeOfDay = time.TimeOfDay; 
            var timeFormat = new DateTime(time.Year, time.Month, time.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);
            return timeFormat;
        }
    }
}