using System;
using System.Linq;
using System.Runtime.CompilerServices;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class IncentiveBalanceResult : FineWorkCheckResult
    {
        public IncentiveBalanceResult(bool isSucceed, String message, decimal balance)
            : base(isSucceed, message)
        {
            this.Balance = balance;
        }

        public decimal Balance { get; set; }

        public static IncentiveBalanceResult Check(ITaskIncentiveManager taskIncentiveManager,
            IAnncIncentiveManager anncIncentiveManager, IIncentiveManager incentiveManager, Guid taskId,
            int incentiveKind,Guid? anncId=null)
        {
            var kind = taskIncentiveManager.FindTaskIncentiveByTaskIdAndKindId(taskId, incentiveKind);
            //计算对应激励的总额
            var gross = taskIncentiveManager.FindTaskIncentiveByTaskIdAndKindId(taskId, incentiveKind).Amount;


            //任务作为红包发放的值
            var hg =
                incentiveManager.FetchIncentiveByTaskId(taskId)
                    .Where(p => p.TaskIncentive.IncentiveKind.Id == incentiveKind)
                    .Sum(p => p.Quantity);

            //未验证的通告预先定义的激励数
            var anncIncentives =
                anncIncentiveManager.FetchAnncIncentiveByTaskIdAndKind(taskId, incentiveKind)
                    .Where(p => p.Announcement.Reviews.All(a => a.Reviewstatus != ReviewStatuses.Approved));
            if (anncId.HasValue)
                anncIncentives = anncIncentives.Where(p => p.Announcement.Id != anncId.Value);


            var balance = gross - hg - anncIncentives.Sum(p=>p.Amount);
            

            return Check(balance, $"任务{kind.IncentiveKind.Name}余额不足.");

        }

        private static IncentiveBalanceResult Check(decimal balance, String message)
        {
            if (balance <= 0)
            {
                return new IncentiveBalanceResult(false, message, balance);
            }

            return new IncentiveBalanceResult(true, null, balance);
        }
    }
}
