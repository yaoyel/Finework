using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查账号是否不是任务的参与者. </summary>
    public class AccountIsNotPartakerResult : FineWorkCheckResult
    {
        public AccountIsNotPartakerResult(bool isSucceed, string message, PartakerEntity partaker)
            : base(isSucceed, message)
        {
            this.Partaker = partaker;
        }

        public PartakerEntity Partaker { get; private set; }

        public static AccountIsNotPartakerResult Check(TaskEntity task, Guid accountId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var partaker = task.Partakers.SingleOrDefault(x => x.Staff.Account.Id == accountId);
            if (partaker != null)
            {
                return new AccountIsNotPartakerResult(false, "用户已经是项目成员", partaker);
            }
            return new AccountIsNotPartakerResult(true, null, null);
        } 
    }
}