using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskAlarmManager
    {
        TaskAlarmEntity CreateTaskAlarm(CreateTaskAlarmModel taskAlarmModel);

        IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByResolvedStatus(Guid taskId, Guid staffId,bool isResolved);

        IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByTaskId(Guid taskId);

        TaskAlarmEntity FindTaskAlarm(Guid taskAlarmId);
         
        TaskAlarmEntity ChangeResolvedStatus(Guid taskAlarmId, Guid accountId, ResolveStatus newStatus,string comment);

        IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByCreatorId(Guid staffId,bool includeGreenLight=false, bool includeAllTask = false);

        IEnumerable<TaskAlarmEntity> FetchAlarmsWithPartakerByStaffAndKind(Guid staffId, TaskAlarmKinds? alarmKind);

        void UpdateTaskAlarm(TaskAlarmEntity taskAlarm);
         
        IEnumerable<TaskAlarmEntity> FetchAlarmsByConversationId(string conversationId);  

        IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByStaffIdWithTaskId( Guid staffId, Guid? taskId);

         
    }
}
