using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IAlarmManager
    {
        AlarmEntity CreateAlarmPeriod(CreateAlarmPeriodModel createAlarmPeriodModel);

        AlarmEntity CreateAlarmPeriodByTemp(Guid alarmTempId);

        AlarmEntity UpdateAlarmPeriodTime(UpdateAlarmPeriodModel updateAlarmPeriodModel);

        AlarmEntity UpdateAlarmPeriodEnabled(Guid alarmPeriodId, bool isEnabled);
         
        void DeleteAlarmPeriod(AlarmEntity alarmPeriod);

        void DeleteAlarmPeriodsByTaskId(Guid taskId);

        AlarmEntity FindAlarmPeriod(Guid alarmPeriodId);

        IEnumerable<AlarmEntity> FetchAllAlarmPeriods();

        IEnumerable<AlarmEntity> FetchAlarmPeriodsByTaskId(Guid taskId);

        IEnumerable<AlarmEntity> FetchAlarmPeriodsByTime(DateTime? date);

        IEnumerable<AlarmEntity> FetchUntreatedAlarmPeriodByStaff(Guid staffId); 


    }
}
