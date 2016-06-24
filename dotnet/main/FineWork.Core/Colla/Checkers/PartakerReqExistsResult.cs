using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查任务申请是否存在. </summary>
    public class PartakerReqExistsResult : FineWorkCheckResult
    {
        public PartakerReqExistsResult(bool isSucceed, String message, [CanBeNull] PartakerReqEntity partakerReqEntity)
            : base(isSucceed, message)
        {
            this.PartakerReq = partakerReqEntity;
        }

        public PartakerReqEntity PartakerReq { get; private set; }

        public static PartakerReqExistsResult Check(IPartakerReqManager partakerReqManager, Guid partakerReqId)
        {
            if (partakerReqManager == null) throw new ArgumentNullException(nameof(partakerReqManager));
            var existed = partakerReqManager.FindPartakerReq(partakerReqId);
            if (existed == null)
            {
                return new PartakerReqExistsResult(false, "不存在对应的申请信息.", null);
            }
            return new PartakerReqExistsResult(true, null, existed);
        }

        public static PartakerReqExistsResult CheckPending(IPartakerReqManager partakerReqManager, TaskEntity task, StaffEntity staff)
        {
            if (partakerReqManager == null) throw new ArgumentNullException(nameof(partakerReqManager));
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            var pendingReqs = partakerReqManager.FetchPendingPartakerReqs(task.Id, staff.Id);
            var existed = pendingReqs.SingleOrDefault();

            if (existed == null)
            {
                return new PartakerReqExistsResult(false,"不存在对应的申请信息.", null);
            }
            return new PartakerReqExistsResult(true, null, existed);
        }
    }
}
