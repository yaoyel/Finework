using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class IncentiveKindExistsResult: FineWorkCheckResult
    {
        public IncentiveKindExistsResult(bool isSucceed, string message, IncentiveKindEntity incentiveKind)
            :base(isSucceed,message)
        {
            this.IncentiveKind = incentiveKind;
        }

        public IncentiveKindEntity IncentiveKind { get; private set; }

        public static IncentiveKindExistsResult Check(IIncentiveKindManager incentiveKindManager,int kindId)
        {
            if (incentiveKindManager == null) throw new ArgumentException(nameof(incentiveKindManager));

            var incentiveKind = incentiveKindManager.FindIncentiveKindById(kindId);
            return Check(incentiveKind, $"激励类型:{kindId} 不存在.");
        }

        private static IncentiveKindExistsResult Check([CanBeNull] IncentiveKindEntity incentiveKind, string message)
        {
            if (incentiveKind == null)
                return new IncentiveKindExistsResult(false, message, null);
            return new IncentiveKindExistsResult(true, null, incentiveKind);
        }
    }
}
