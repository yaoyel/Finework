using System;
using System.Linq;
using AppBoot.Checks;
using JetBrains.Annotations;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="PartakerEntity"/> 是否存在. </summary>
    public class PartakerExistsResult : FineWorkCheckResult
    {
        public PartakerExistsResult(bool isSucceed, String message, PartakerEntity partaker)
            : base(isSucceed, message)
        {
            this.Partaker = partaker;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="PartakerEntity"/>, 否则为 <c>null</c>. </summary>
        public PartakerEntity Partaker { get; private set; }

        /// <summary> 根据 <see cref="PartakerEntity.Id"/> 返回是否存在相应的 <see cref="PartakerEntity"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static PartakerExistsResult Check(TaskEntity task, Guid partakerId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            PartakerEntity partaker = task.Partakers.SingleOrDefault(x => x.Id == partakerId);
            return Check(partaker, "不存在对应的任务成员信息.");
        }

        public static PartakerExistsResult CheckForStaff(TaskEntity task, Guid staffId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            PartakerEntity partaker = task.Partakers.SingleOrDefault(x => x.Staff.Id == staffId);
            return Check(partaker, "不存在对应的任务成员信息.");
        } 

        private static PartakerExistsResult Check([CanBeNull] PartakerEntity partaker, String message)
        {
            if (partaker == null)
            {
                return new PartakerExistsResult(false, message, null);
            }
            return new PartakerExistsResult(true, null, partaker);
        }
    }
}