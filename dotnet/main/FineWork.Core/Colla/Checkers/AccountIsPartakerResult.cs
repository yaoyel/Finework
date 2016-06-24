using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查账号是否为任务的参与者. </summary>
    public class AccountIsPartakerResult : FineWorkCheckResult
    {
        public AccountIsPartakerResult(bool isSucceed, string message, PartakerEntity partaker)
            : base(isSucceed, message)
        {
            this.Partaker = partaker;
        }

        public PartakerEntity Partaker { get; private set; }

        public static AccountIsPartakerResult Check(TaskEntity task, Guid accountId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var partaker = task.Partakers.SingleOrDefault(x => x.Staff.Account.Id == accountId);
            if (partaker == null)
            {
                return new AccountIsPartakerResult(false, "用户不是项目成员", null);
            }
            return new AccountIsPartakerResult(true, null, partaker);
        }

        public static AccountIsPartakerResult Check(TaskEntity task, Guid accountId, PartakerKinds kind)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var partaker = task.Partakers.SingleOrDefault(x => x.Staff.Account.Id == accountId);
            if (partaker == null)
            {
                return new AccountIsPartakerResult(false, "用户不是项目成员", null);
            }
            if (partaker.Kind != kind)
                return new AccountIsPartakerResult(false, $"用户不是项目{EnumUtil.GetLabel(kind)}", null);

            return new AccountIsPartakerResult(true, null, partaker);
        }

    }
}
