using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IAlarmManager
    {
        AlarmEntity CreateAlarmPeriod(TaskEntity task, int date, string shortTime,string bell,bool isEnabled=true);

        AlarmEntity UpdateAlarmPeriodTime(Guid alarmPeriodId, int date, string shortTime,string bell);

        AlarmEntity UpdateAlarmPeriodEnabled(Guid alarmPeriodId, bool isEnabled);
         
        void DeleteAlarmPeriod(AlarmEntity alarmPeriod);

        AlarmEntity FindAlarmPeriod(Guid alarmPeriodId);

        IEnumerable<AlarmEntity> FetchAllAlarmPeriods();

        IEnumerable<AlarmEntity> FetchAlarmPeriodsByTaskId(Guid taskId);

        IEnumerable<AlarmEntity> FetchAlarmPeriodsByDate(DateTime? date);

        IEnumerable<AlarmEntity> FetchUntreatedAlarmPeriodByStaff(Guid staffId); 
    }
}
