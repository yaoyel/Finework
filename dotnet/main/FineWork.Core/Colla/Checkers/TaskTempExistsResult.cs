using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class TaskTempExistsResult : FineWorkCheckResult
    {
        public TaskTempExistsResult(bool isSucceed, String message, TaskTempEntity taskTemp)
            : base(isSucceed, message)
        {
            this.TaskTemp = taskTemp;
        }

        public TaskTempEntity TaskTemp { get; set; }
        public static TaskTempExistsResult Check(ITaskTempManager taskTempManager, Guid taskTempId)
        {
            var taskTemp = taskTempManager.FindById(taskTempId);
            return Check(taskTemp, "不存在对应的任务模板");
        }

        public static TaskTempExistsResult CheckForTask(ITaskTempManager taskTempManager, Guid taskId)
        {
            var taskTemp = taskTempManager.FindByTaskId(taskId);
            return Check(taskTemp, "不存在对应的任务模板");
        }

        private static TaskTempExistsResult Check([CanBeNull]  TaskTempEntity taskTemp, String message)
        {
            if (taskTemp == null)
            {
                return new TaskTempExistsResult(false, message, null);
            }

            return new TaskTempExistsResult(true, null, taskTemp);
        }
    }
}
