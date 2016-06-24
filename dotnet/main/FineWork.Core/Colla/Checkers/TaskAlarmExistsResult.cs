using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using FineWork.Security;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class TaskAlarmExistsResult : FineWorkCheckResult
    {
        public TaskAlarmExistsResult(bool isSucceed, String message, TaskAlarmEntity taskAlarm)
            : base(isSucceed, message)
        {
            this.TaskAlarm = taskAlarm;
        }

        public TaskAlarmEntity TaskAlarm { get; private set; }

        public static TaskAlarmExistsResult Check(ITaskAlarmManager taskAlarmManager, Guid taskAlarmId)
        {
            TaskAlarmEntity alarm = taskAlarmManager.FindTaskAlarm(taskAlarmId);
            return Check(alarm, "不存在对应的任务预警信息.");
        }

        public static TaskAlarmExistsResult CheckByReceivers(ITaskAlarmManager taskAlarmManager,Guid taskId,Guid staffId, int[] receiverKinds)
        {
            var alarm = taskAlarmManager.FindTaskAlarmByReceiverKinds(taskId, staffId, receiverKinds);
            return Check(alarm, "不存在对应的任务预警信息.");
        }


        private static TaskAlarmExistsResult Check([CanBeNull] TaskAlarmEntity taskAlarm, String message)
        {
            if (taskAlarm == null)
            {
                return new TaskAlarmExistsResult(false, message, null);
            }
            return new TaskAlarmExistsResult(true, null, taskAlarm);
        }
    }
}