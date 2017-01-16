using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using System.Data.Entity;
using System.Security.Cryptography.X509Certificates;

namespace FineWork.Colla.Impls
{
    public class PlanAlarmManager : AefEntityManager<PlanAlarmEnitty, Guid>, IPlanAlarmManager
    {
        public PlanAlarmManager(ISessionProvider<AefSession> sessionProvider,
            IPlanManager planManager) : base(sessionProvider)
        {
            Args.NotNull(planManager, nameof(planManager));
            m_PlanManager = planManager;
        }

        private readonly IPlanManager m_PlanManager;
        public PlanAlarmEnitty CreatePlanAlarm(CreatePlanAlarmModel alarmModel)
        {
            Args.NotNull(alarmModel, nameof(alarmModel));

            var plan = PlanExistsResult.Check(m_PlanManager, alarmModel.PlanId.Value).ThrowIfFailed().Plan;
            var alarm=new PlanAlarmEnitty();
            alarm.Id=Guid.NewGuid();
            alarm.Plan = plan;
            alarm.Time = alarmModel.Time;
            alarm.Bell = alarmModel.Bell;
            alarm.IsEnabled = alarmModel.IsEnabled;
            alarm.BeforeStart = alarmModel.BeforeStartMins;

            this.InternalInsert(alarm);
            return alarm;
        }

        public void DeletePlanAlarmByPlanId(Guid planId)
        {
            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan;
            if (plan.Alarms.Any())
            {
                foreach (var alarm in plan.Alarms.ToList())
                {
                    this.InternalDelete(alarm);
                }
            }
        }

        public void DeletePlanAlarmByAlarmId(Guid planAlarmId)
        {
            var planAlarm = PlanAlarmExistsResult.Check(this, planAlarmId).PlanAlarm;
            if(planAlarm!=null)
                this.InternalDelete(planAlarm);
        }

        public void UpdatePlanAlarm(PlanAlarmEnitty alarm)
        {
            Args.NotNull(alarm, nameof(alarm));
            this.InternalUpdate(alarm);
            
        }

        public void UpdatePlanAlarm(UpdatePlanAlarmModel alarmModel)
        {
            Args.NotNull(alarmModel, nameof(alarmModel));

            if (alarmModel.PlanAlarmId != null)
            {
                var planAlarm = PlanAlarmExistsResult.Check(this, alarmModel.PlanAlarmId.Value).PlanAlarm;
                planAlarm.BeforeStart = alarmModel.BeforeStartMins;
                planAlarm.Bell = alarmModel.Bell;
                planAlarm.IsEnabled = alarmModel.IsEnabled;
                planAlarm.Time = alarmModel.Time;

                this.InternalUpdate(planAlarm);
            }
        } 
        public PlanAlarmEnitty FindById(Guid planAlarmId)
        {
            return this.InternalFind(planAlarmId);
        }

        public List<PlanAlarmEnitty> FetchPlanAlarmsByTime(DateTime time)
        {
            var context = this.Session.DbContext;
            var set = context.Set<PlanAlarmEnitty>().Include(p=>p.Plan.Creator.Org)
                .Include(p => p.Plan.Creator.Account).AsNoTracking().AsEnumerable();

            //换算成东八区时间 
            TimeZoneInfo local = TimeZoneInfo.Local;
            time = time.AddHours(8 - local.BaseUtcOffset.Hours);

            var timeOfDay = time.TimeOfDay;

            var timeFormat = new DateTime(time.Year, time.Month, time.Day, timeOfDay.Hours, timeOfDay.Minutes, 0); 

            return
             set.Where(p => p.Time == timeFormat || AlarmtimeMatch(p.Plan,p,timeFormat)).ToList();
        }

        bool AlarmtimeMatch(PlanEntity plan, PlanAlarmEnitty planAlarm,DateTime time)
        {
            return plan.StartAt.HasValue && planAlarm.BeforeStart.HasValue &&
                   plan.StartAt.Value.AddMinutes(-planAlarm.BeforeStart.Value) == time;
        }

    }
}