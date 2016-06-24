using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="PartakerEntity"/> 是否<b>不</b>存在. </summary>
    public class PartakerNotExistsResult : FineWorkCheckResult
    {
        public PartakerNotExistsResult(bool isSucceed, String message, PartakerEntity partaker)
            : base(isSucceed, message)
        {
            this.Partaker = partaker;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="PartakerEntity"/>, 否则为 <c>null</c>. </summary>
        public PartakerEntity Partaker { get; private set; }

        /// <summary> 根据 <see cref="PartakerEntity.Task"/> 与 <see cref="PartakerEntity.Staff"/> 检查是否<b>不</b>存在相应的 <see cref="PartakerEntity"/>. </summary>
        public static PartakerNotExistsResult CheckStaff(TaskEntity task, Guid staffId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            PartakerEntity partaker = task.Partakers.SingleOrDefault(x => x.Staff.Id == staffId);

            var message =$"{partaker?.Staff.Name}已经是任务成员";

            return Check(partaker, message);
        }

        /// <summary> 根据 <see cref="PartakerEntity.Task"/> 与 <see cref="PartakerEntity.Staff"/> 检查是否<b>不</b>存在相应的 <see cref="PartakerEntity"/>. </summary>
        public static PartakerNotExistsResult CheckStaff(TaskEntity task, StaffEntity staff)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (staff == null) throw new ArgumentNullException(nameof(staff));
            return CheckStaff(task, staff.Id);
        }

        private static PartakerNotExistsResult Check(PartakerEntity partaker, string message)
        {
            if (partaker != null)
            {
                return new PartakerNotExistsResult(false, message, partaker);
            }
            return new PartakerNotExistsResult(true, null, null);
        }
    }
}