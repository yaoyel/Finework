using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class PlanAlarmExistsResult : FineWorkCheckResult
    {
        public PlanAlarmExistsResult(bool isSucceed, String message, PlanAlarmEnitty planAlarm)
            : base(isSucceed, message)
        {
            this.PlanAlarm = planAlarm;
        }

        public PlanAlarmEnitty PlanAlarm { get; set; }
        public static PlanAlarmExistsResult Check(IPlanAlarmManager planAlarmManager, Guid alarmId)
        {
            var alarm = planAlarmManager.FindById(alarmId);
            return Check(alarm, null);
        }
 
        private static PlanAlarmExistsResult Check([CanBeNull]  PlanAlarmEnitty planAlarm, String message)
        {
            if (planAlarm == null)
            {
                return new PlanAlarmExistsResult(false, message, null);
            }

            return new PlanAlarmExistsResult(true, null, planAlarm);
        }
    }
}
