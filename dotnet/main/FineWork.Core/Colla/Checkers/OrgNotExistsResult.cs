using System;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="OrgEntity"/> 是否<b>不</b>存在. </summary>
    public class OrgNotExistsResult : FineWorkCheckResult
    {
        public OrgNotExistsResult(bool isSucceed, String message, OrgEntity org)
            : base(isSucceed, message)
        {
            this.Org = org;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="OrgEntity"/>, 否则为 <c>null</c>. </summary>
        public OrgEntity Org { get; private set; }

        /// <summary> 根据 <see cref="OrgEntity.Name"/> 检查是否<b>不</b>存在相应的 <see cref="OrgEntity"/>. </summary>
        public static OrgNotExistsResult Check(IOrgManager orgManager, String orgName)
        {
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (String.IsNullOrEmpty(orgName)) throw new ArgumentNullException(nameof(orgName));

            OrgEntity org = orgManager.FindOrgByName(orgName);
            return Check(org, $"已存在名称为[{orgName}]的组织.");
        }

        private static OrgNotExistsResult Check(OrgEntity org, string message)
        {
            if (org != null)
            {
                return new OrgNotExistsResult(false, message, org);
            }
            return new OrgNotExistsResult(true, null, null);
        }
    }
}