using AppBoot.Checks;
using FineWork.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Checkers
{
    public class InvitationIsEnabledResult : FineWorkCheckResult
    {
        private InvitationIsEnabledResult(bool isSucceed, String message)
            : base(isSucceed, message)
        {

        }
        public static InvitationIsEnabledResult Check(IOrgManager orgManager, Guid orgId)
        {
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            var org = orgManager.FindOrg(orgId);

            return Check(org, "当前组织不允许员工要求成员，请联系管理员.");
        }

        private static InvitationIsEnabledResult Check(OrgEntity org, String message)
        {
            if (org != null && org.IsInvEnabled)
                return new InvitationIsEnabledResult(true, null);

            return new InvitationIsEnabledResult(false, message);
        }
    }
}