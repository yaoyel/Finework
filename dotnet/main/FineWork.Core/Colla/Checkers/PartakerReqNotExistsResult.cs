using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查任务申请是否不存在. </summary>
    public class PartakerReqNotExistsResult : FineWorkCheckResult
    {
        public PartakerReqNotExistsResult(bool isSucceed, String message, [CanBeNull] PartakerReqEntity partakerReqEntity)
            : base(isSucceed, message)
        {
            this.PartakerReq = partakerReqEntity;
        }

        public PartakerReqEntity PartakerReq { get; private set; }

        public static PartakerReqNotExistsResult Check(IPartakerReqManager partakerReqManager, TaskEntity task, StaffEntity staff)
        {
            if (partakerReqManager == null) throw new ArgumentNullException(nameof(partakerReqManager));
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            var pendingReqs = partakerReqManager.FetchPendingPartakerReqs(task.Id, staff.Id);
            var existed = pendingReqs.SingleOrDefault();

            if (existed != null)
            {
                return new PartakerReqNotExistsResult(false, "已经存在对应的申请信息.", null);
            }
            return new PartakerReqNotExistsResult(true, null, null);
        }
    }
}