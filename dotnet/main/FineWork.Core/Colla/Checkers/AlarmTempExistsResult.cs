using System;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class AlarmTempExistsResult : FineWorkCheckResult
    {
        public AlarmTempExistsResult(bool isSucceed, string message, AlarmTempEntity alarmPeriod)
            :base(isSucceed,message)
        {
            this.AlarmPeriod = alarmPeriod;
        }

        public AlarmTempEntity AlarmPeriod { get; private set; }

        public static AlarmTempExistsResult Check(IAlarmTempManager alarmManager, Guid alarmPeriodId)
        {
            if (alarmManager == null) throw new ArgumentException(nameof(alarmManager));

            var alarmPeriod = alarmManager.FindById(alarmPeriodId);
            return Check(alarmPeriod, "不存在对应的闹钟。");
        }

        private static AlarmTempExistsResult Check(AlarmTempEntity alarmPeriod, string message)
        {
            if (alarmPeriod == null)
                return new AlarmTempExistsResult(false, message, null);
            return new AlarmTempExistsResult(true, null, alarmPeriod);
        }
    }
}
