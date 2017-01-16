using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FineWork.Colla
{
    public interface IPushLogManager
    {
        PushLogEntity CreatePushLog(Guid staffId, Guid targetId, PushKinds pushKind);

        PushLogEntity FindById(Guid pushLogId);

        void DeletePushLogs(params Guid[] pushLogIds);

        void DeletePushLogsByAnnc(Guid anncId);

        void DeletePushLogsByAlarm(Guid alarmId);

        void DeletePushLogsByPlan(Guid planId);

        void DeletePushLogsByTargetId(Guid targetId);

        void DeletePushLogsByStaffId(Guid staffId,PushKinds[] pushKinds);

        void ChanageViewedStatus(Guid pushLogId);

        IEnumerable<PushLogEntity> FetchPushLogsByStaffId(Guid staffId);

        IEnumerable<PushLogEntity> FetchPushLogsByKinds(Guid staffId,PushKinds[] pushKinds);
    }
}