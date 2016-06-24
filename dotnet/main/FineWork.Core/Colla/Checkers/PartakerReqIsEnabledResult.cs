using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查是否有权申请加入任务. </summary>
    public class PartakerReqIsEnabledResult : FineWorkCheckResult
    {
        public PartakerReqIsEnabledResult(bool isSucceed, String message, TaskEntity task, PartakerKinds kind)
            :base(isSucceed, message)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            this.Task = task;
            this.Kind = kind;
        }

        public TaskEntity Task { get; private set; }

        public PartakerKinds Kind { get; private set; }

        public static PartakerReqIsEnabledResult Check(ITaskManager taskManager, Guid taskId, PartakerKinds kind)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));

            TaskEntity task = TaskExistsResult.Check(taskManager, taskId).ThrowIfFailed().Task;
            return Check(task, kind);
        }

        /// <summary> 检查 <paramref name="task"/> 是否接受 <see cref="PartakerKinds"/> 为 <paramref name="kind"/> 的申请. </summary>
        /// <returns> 接受时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PartakerReqIsEnabledResult Check(TaskEntity task, PartakerKinds kind)
        {
            if (IsAcceptable(task, kind))
            {
                return new PartakerReqIsEnabledResult(true, null, task, kind);
            }
            return new PartakerReqIsEnabledResult(false, $"[{task.Name}]不招募{kind.GetLabel()}.", task, kind);
        }

        public override Exception CreateException(string message)
        {
            return new FineWorkException(message);
        }

        private static bool IsAcceptable(TaskEntity task, PartakerKinds partakerKind)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var recuitmentRoles = task.RecruitmentRoles;

            return task.IsRecruitEnabled && recuitmentRoles!=null &&  recuitmentRoles.Contains(((int) partakerKind).ToString());

        }
    }
}
