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

        public static IncentiveBalanceResult Check(ITaskIncentiveManager taskIncentiveManager,  IIncentiveManager incentiveManager, Guid taskId,
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

            var balance = gross - hg;
            

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
