using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查任务邀请是否存在. </summary>
    public class PartakerInvNotExistsResult : FineWorkCheckResult
    {
        public PartakerInvNotExistsResult(bool isSucceed, String message, [CanBeNull] PartakerInvEntity partakerInvEntity)
            : base(isSucceed, message)
        {
            this.PartakerInv = partakerInvEntity;
        }

        public PartakerInvEntity PartakerInv { get; private set; }

        public static PartakerInvNotExistsResult Check(IPartakerInvManager partakerInvManager, TaskEntity task, StaffEntity staff,PartakerKinds kind)
        {
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager));
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            var pendingInvs = partakerInvManager.FetchPendingPartakerInvs(task.Id, staff.Id);
            var existed = pendingInvs.FirstOrDefault(p => p.PartakerKind == kind);

            if (existed != null)
            {
                return new PartakerInvNotExistsResult(false,"已经存在该任务的邀请.",null);
            }
            return new PartakerInvNotExistsResult(true, null, null);
        }
    }
}