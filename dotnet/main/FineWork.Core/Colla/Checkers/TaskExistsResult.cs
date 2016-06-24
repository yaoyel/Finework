using System;
using AppBoot.Checks;
using JetBrains.Annotations;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="TaskEntity"/> 是否存在. </summary>
    public class TaskExistsResult : FineWorkCheckResult
    {
        public TaskExistsResult(bool isSucceed, String message, TaskEntity task)
            : base(isSucceed, message)
        {
            this.Task = task;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="TaskEntity"/>, 否则为 <c>null</c>. </summary>
        public TaskEntity Task { get; private set; }

        /// <summary> 根据 <see cref="TaskEntity.Id"/> 返回是否存在相应的 <see cref="TaskEntity"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static TaskExistsResult Check(ITaskManager taskManager, Guid taskId)
        {
            TaskEntity task = taskManager.FindTask(taskId);
            return Check(task, "不存在对应的任务.");
        }

        private static TaskExistsResult Check([CanBeNull] TaskEntity task, String message)
        {
            if (task == null)
            {
                return new TaskExistsResult(false, message, null);
            }
            return new TaskExistsResult(true, null, task);
        }
    }
}