using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IPlanAlarmManager
    {
        PlanAlarmEnitty CreatePlanAlarm(CreatePlanAlarmModel alarmModel);

        void UpdatePlanAlarm(PlanAlarmEnitty alarm);

        void DeletePlanAlarmByPlanId(Guid planId);

        void DeletePlanAlarmByAlarmId(Guid planAlarmId);

        void UpdatePlanAlarm(UpdatePlanAlarmModel alarmModel);

        PlanAlarmEnitty FindById(Guid planAlarmId);

        List<PlanAlarmEnitty> FetchPlanAlarmsByTime(DateTime time);
    }
}