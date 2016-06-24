using System;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="TaskEntity"/> 是否<b>不</b>存在. </summary>
    public class TaskNotExistsResult : FineWorkCheckResult
    {
        public TaskNotExistsResult(bool isSucceed, String message, TaskEntity task)
            : base(isSucceed, message)
        {
            this.Task = task;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="TaskEntity"/>, 否则为 <c>null</c>. </summary>
        public TaskEntity Task { get; private set; }

        /// <summary> 根据 <see cref="TaskEntity.Name"/> 检查在一个组织的顶级任务中是否<b>不</b>存在相应的 <see cref="TaskEntity"/>. </summary>
        public static TaskNotExistsResult CheckInOrg(ITaskManager taskManager, Guid orgId, String taskName,Guid? taskId=null)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (String.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));

            TaskEntity task = taskManager.FindTaskByNameInOrg(orgId, taskName);
            if (task!=null && taskId.HasValue && task.Id == taskId.Value)
                task = null;
            return Check(task, $"已存在名称为[{taskName}]的任务.");
        }

        /// <summary> 根据 <see cref="TaskEntity.Name"/> 检查在一个组织的顶级任务中是否<b>不</b>存在相应的 <see cref="TaskEntity"/>. </summary>
        public static TaskNotExistsResult CheckInParent(ITaskManager taskManager, Guid parentTaskId, String taskName)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (String.IsNullOrEmpty(taskName)) throw new ArgumentNullException(nameof(taskName));

            TaskEntity task = taskManager.FindTaskByNameInParent(parentTaskId, taskName);
            return Check(task, $"已存在名称为[{taskName}]的任务.");
        }

        private static TaskNotExistsResult Check(TaskEntity task, String message)
        {
            if (task != null)
            {
                return new TaskNotExistsResult(false, message, task);
            }
            return new TaskNotExistsResult(true, null, null);
        }
    }
}