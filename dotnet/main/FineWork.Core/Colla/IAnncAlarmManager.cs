using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IAnncAlarmManager
    {
        AnncAlarmEntity CreateAnnAlarm(CreateAnncAlarmModel createAnncAlarmModel);

        AnncAlarmEntity UpdateAnncAlarm(UpdateAnncAlarmModel updateAnncAlarmModel);

        void DeleteAnncAlarm(Guid anncAlarmId);

        void DeleteAnncAlarmByAnncId(Guid anncId);

        AnncAlarmEntity FindById(Guid anncAlarmId);

        AnncAlarmEntity FindByAnncIdAndTime(Guid anncId, DateTime time);

        IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByAnncId(Guid anncId); 

        IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByStaffId(Guid staffId); 

        IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByTime(DateTime touchTime); 

        void UpdateAnncAlarmStatus(Guid anncAlarmId, bool isEnabled);

    }
}