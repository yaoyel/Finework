using System;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查是否有权邀请他人加入任务. </summary>
    public class PartakerInvIsEnabledResult : FineWorkCheckResult
    {
        public PartakerInvIsEnabledResult(bool isSucceed, String message, TaskEntity task, PartakerEntity inviterPartaker, PartakerKinds kind)
            :base(isSucceed, message)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (inviterPartaker == null) throw new ArgumentNullException(nameof(inviterPartaker));

            this.Task = task;
            this.InviterPartaker = inviterPartaker;
            this.Kind = kind;
        }

        public TaskEntity Task { get; private set; }

        public PartakerEntity InviterPartaker { get; private set; }

        public PartakerKinds Kind { get; private set; }

        /// <summary> 检查 <paramref name="inviterPartaker"/> 是否可以邀请他人作为 <see cref="PartakerKinds"/> 加入任务 <paramref name="task"/>. </summary>
        /// <returns> 有权时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PartakerInvIsEnabledResult Check(TaskEntity task, PartakerEntity inviterPartaker, PartakerKinds kind)
        {
            if (IsEnabled(task, inviterPartaker, kind))
            {
                return new PartakerInvIsEnabledResult(true, null, task, inviterPartaker, kind);
            }
            return new PartakerInvIsEnabledResult(false, 
                $"[{inviterPartaker.Staff.Name}] 无权邀请他人在任务 [{task.Name}] 中担任 [{kind.GetLabel()}].", 
                task, inviterPartaker, kind);
        }

        public override Exception CreateException(string message)
        {
            return new FineWorkException(message);
        }

        private static bool IsEnabled(TaskEntity task, PartakerEntity inviterPartaker, PartakerKinds partakerKind)
        { 

            if (task == null) throw new ArgumentNullException(nameof(task));
            if (inviterPartaker == null) throw new ArgumentNullException(nameof(inviterPartaker));

            if (inviterPartaker.Kind == PartakerKinds.Leader)
                return true;

            switch (partakerKind)
            {
                case PartakerKinds.Mentor:
                    return task.IsMentorInvEnabled;
                case PartakerKinds.Collaborator:
                    return task.IsCollabratorInvEnabled;
            }
 
            switch(inviterPartaker.Kind)
            { 
                case PartakerKinds.Recipient:
                    return true;
                case PartakerKinds.Mentor:
                    return true;
                case PartakerKinds.Collaborator:
                    return partakerKind == PartakerKinds.Collaborator;
                default:
                    return false;
            }
             
        }
    }
}