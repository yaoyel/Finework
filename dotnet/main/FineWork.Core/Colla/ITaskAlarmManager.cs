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

        TaskAlarmEntity FindTaskAlarmByReceiverKinds(Guid taskId, Guid staffId, int[] receivers);

        TaskAlarmEntity ChangeResolvedStatus(Guid taskAlarmId, Guid accountId, ResolveStatus newStatus,string comment);

        IEnumerable<TaskAlarmEntity> FetchTaskAlarmsByStaffId(Guid staffId,bool includeGreenLight=false, bool includeAllTask = false);

        IEnumerable<TaskAlarmEntity> FetchAlarmsWithPartakerByStaffAndKind(Guid staffId, TaskAlarmKinds? alarmKind);

        void UpdateTaskAlarm(TaskAlarmEntity taskAlarm);

        IEnumerable<TaskAlarmEntity> FetchAlarmsByChatRoomKind(Guid taskId, ChatRoomKinds roomKind,Guid creatorStaffId);

        IEnumerable<TaskAlarmEntity> FetchAlarmsByConversationId(string conversationId);

        IEnumerable<TaskAlarmEntity> FetchAlarmsByPartakerKind(Guid taskId, PartakerKinds partakerKind);

        void UpdateRemoteServer();

    }
}
