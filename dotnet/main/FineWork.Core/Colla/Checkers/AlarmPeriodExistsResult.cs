using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class AlarmExistsResult:FineWorkCheckResult
    {
        public AlarmExistsResult(bool isSucceed, string message, AlarmEntity alarmPeriod)
            :base(isSucceed,message)
        {
            this.AlarmPeriod = alarmPeriod;
        }

        public AlarmEntity AlarmPeriod { get; private set; }

        public static AlarmExistsResult Check(IAlarmManager alarmManager, Guid alarmPeriodId)
        {
            if (alarmManager == null) throw new ArgumentException(nameof(alarmManager));

            var alarmPeriod = alarmManager.FindAlarmPeriod(alarmPeriodId);
            return Check(alarmPeriod, "不存在对应的闹钟。");
        }

        private static AlarmExistsResult Check(AlarmEntity alarmPeriod, string message)
        {
            if (alarmPeriod == null)
                return new AlarmExistsResult(false, message, null);
            return new AlarmExistsResult(true, null, alarmPeriod);
        }
    }
}
