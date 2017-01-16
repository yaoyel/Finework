using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IAnncAlarmRecManager
    {
        AnncAlarmRecEntity CreateAnncAlarmRec(Guid anncAlarmId, Guid staffId, AnncRoles anncRole);

        void DeleteRecByAnncId(Guid anncId, Guid staffId, AnncRoles anncRole);

        IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncAlarmId(Guid anncAlarmId);

        void DeleteAnncAlarmRecByAlarmId(Guid anncAlarmId);

        IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncIdWithStaffId(Guid anncId, Guid staffId, AnncRoles role);

        IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncIdWithRole(Guid anncId,  AnncRoles role);

        void UpdateAnncAlarmRec(AnncAlarmRecEntity rec);
    }
}