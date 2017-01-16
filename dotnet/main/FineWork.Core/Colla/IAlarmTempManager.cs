using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IAlarmTempManager
    {
        AlarmTempEntity CreateAlarmTemp(CreateAlarmPeriodModel model);

        void DeleteAlarmTemps(params Guid[] alarmTempIds);

        void UpdateAlarmTemp(UpdateAlarmPeriodModel model);

        AlarmTempEntity FindById(Guid alarmTmepId);

        void UpdateAlarmTempEnabled(Guid alarmTempId, bool isEnabled);

        IEnumerable<AlarmTempEntity> FetchOverdueAlarmTemps();
    }
}