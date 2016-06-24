using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla
{
    public class InvCodeExistResult : FineWorkCheckResult
    {
        public InvCodeExistResult(bool isSucceed, String message, InvCodeEntity code)
            : base(isSucceed, message)
        {
            this.Code = code;
        }

        public InvCodeEntity Code { get; private set; }
        public static InvCodeExistResult Check(IInvCodeManager invCodeManager, string code)
        {
            InvCodeEntity invCode = invCodeManager.FindByCode(code);
            return Check(invCode, "组织邀请码不存在.");
        }


        private static InvCodeExistResult Check([CanBeNull] InvCodeEntity invCode, String message)
        {
            if (invCode == null)
            {
                return new InvCodeExistResult(false, message, null);
            }
            if (invCode?.ExpiredAt != null)
            {
                return new InvCodeExistResult(false, "组织邀请码已被使用.", null);
            }
            return new InvCodeExistResult(true, null, invCode);
        }
    }
}
