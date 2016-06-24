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
    /// <summary> 检查任务邀请是否存在. </summary>
    public class PartakerInvExistsResult : FineWorkCheckResult
    {
        public PartakerInvExistsResult(bool isSucceed, String message, [CanBeNull] PartakerInvEntity partakerInvEntity)
            : base(isSucceed, message)
        {
            this.PartakerInv = partakerInvEntity;
        }

        public PartakerInvEntity PartakerInv { get; private set; }

        public static PartakerInvExistsResult CheckPending(IPartakerInvManager partakerInvManager, TaskEntity task, StaffEntity staff)
        {
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager));
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            var pendingInvs = partakerInvManager.FetchPendingPartakerInvs(task.Id, staff.Id); 
            var existed = pendingInvs.FirstOrDefault();

            if (existed == null)
            {
                return new PartakerInvExistsResult(false,"不存在对应的任务邀请.",null);
            }
            return new PartakerInvExistsResult(true, null, existed);
        }

        public static PartakerInvExistsResult CheckPending(IPartakerInvManager partakerInvManager, TaskEntity task, StaffEntity staff,
            PartakerKinds partakerKind)
        {
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager));
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));

            var pendingInvs = partakerInvManager.FetchPendingPartakerInvs(task.Id, staff.Id);
            var existed = pendingInvs.FirstOrDefault(p => p.PartakerKind == partakerKind);

            if (existed == null)
            {
                return new PartakerInvExistsResult(false, "不存在对应的角色邀请.", null);
            }
            return new PartakerInvExistsResult(true, null, existed);
        }


        public static PartakerInvExistsResult CheckPending(IPartakerInvManager partakerInvManager, Guid partakerInvId)
        {
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager)); 

            var pendingInvs = partakerInvManager.FindPartakerInv(partakerInvId); 

            if (pendingInvs == null)
            {
                return new PartakerInvExistsResult(false,"不存在对应的任务邀请.",null);
            }
            return new PartakerInvExistsResult(true, null, pendingInvs);
        }
    }
}
