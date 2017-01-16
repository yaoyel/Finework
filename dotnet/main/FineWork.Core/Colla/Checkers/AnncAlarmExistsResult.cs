using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class AnncAlarmExistsResult : FineWorkCheckResult
    {
        public AnncAlarmExistsResult(bool isSucceed, String message, AnncAlarmEntity anncAlarm)
            : base(isSucceed, message)
        {
            this.AnncAlarm = anncAlarm;
        }

        public AnncAlarmEntity AnncAlarm { get; set; }
        public static AnncAlarmExistsResult Check(IAnncAlarmManager anncAlarmManager, Guid anncAlarmId)
        {
            var anncAlarm = anncAlarmManager.FindById(anncAlarmId);
            return Check(anncAlarm, "预警不存在.");
        } 
 
        private static AnncAlarmExistsResult Check([CanBeNull]  AnncAlarmEntity anncAlarm, String message)
        {
            if (anncAlarm == null)
            {
                return new AnncAlarmExistsResult(false, message, null);
            }

            return new AnncAlarmExistsResult(true, null, anncAlarm);
        }
    }
}
