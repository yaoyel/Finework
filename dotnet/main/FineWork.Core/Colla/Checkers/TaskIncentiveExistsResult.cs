using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class TaskIncentiveExistsResult:FineWorkCheckResult
    {
        public TaskIncentiveExistsResult(bool isSucceed, string message, TaskIncentiveEntity taskIncentive)
            : base(isSucceed, message)
        {
            this.TaskIncentive = taskIncentive;
        }

         public TaskIncentiveEntity TaskIncentive { get; private set; }

        public static TaskIncentiveExistsResult Check(ITaskIncentiveManager taskIncentiveManager, Guid taskId,
            int kindId)
        {
            if (taskIncentiveManager == null) throw new ArgumentNullException(nameof(taskIncentiveManager));

            var taskIncentive = taskIncentiveManager.FindTaskIncentiveByTaskIdAndKindId(taskId, kindId,true);
            return Check(taskIncentive, "不存在对应的任务激励信息");
        }

        public static TaskIncentiveExistsResult Check(ITaskIncentiveManager taskIncentiveManager, Guid taskIncentiveId)
        {
            if (taskIncentiveManager == null) throw new ArgumentNullException(nameof(taskIncentiveManager));

            var taskIncentive = taskIncentiveManager.FindTaskIncentiveById(taskIncentiveId);
            return Check(taskIncentive, "不存在对应的任务激励信息.");
        }

        private static TaskIncentiveExistsResult Check([CanBeNull]TaskIncentiveEntity taskIncentive, string message)
        {
            if (taskIncentive == null)
                return new TaskIncentiveExistsResult(false, message, null);
            return new TaskIncentiveExistsResult(true, null, taskIncentive);
        }
    }
}
