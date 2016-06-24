using System;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> ����˺��Ƿ�������Ĳ�����. </summary>
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
                return new AccountIsNotPartakerResult(false, "�û��Ѿ�����Ŀ��Ա", partaker);
            }
            return new AccountIsNotPartakerResult(true, null, null);
        } 
    }
}