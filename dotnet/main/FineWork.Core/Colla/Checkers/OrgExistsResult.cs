using System;
using AppBoot.Checks;
using FineWork.Security;
using JetBrains.Annotations;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="OrgEntity"/> 是否存在. </summary>
    public class OrgExistsResult : FineWorkCheckResult
    {
        public OrgExistsResult(bool isSucceed, String message, OrgEntity org)
            : base(isSucceed, message)
        {
            this.Org = org;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="OrgEntity"/>, 否则为 <c>null</c>. </summary>
        public OrgEntity Org { get; private set; }

        /// <summary> 根据 <see cref="OrgEntity.Id"/> 返回是否存在相应的 <see cref="OrgEntity"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static OrgExistsResult Check(IOrgManager orgManager, Guid orgId)
        {
            OrgEntity org = orgManager.FindOrg(orgId);
            return Check(org, "不存在对应的组织.");
        }

        /// <summary> 根据 <see cref="OrgEntity.Name"/> 返回是否存在相应的 <see cref="IAccount"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static OrgExistsResult Check(IOrgManager orgManager, String orgName)
        {
            OrgEntity org = orgManager.FindOrgByName(orgName);
            return Check(org, $"名称为 [{orgName}]的组织不存在.");

        }

        private static OrgExistsResult Check([CanBeNull] OrgEntity org, String message)
        {
            if (org == null)
            {
                return new OrgExistsResult(false, message, null);
            }
            return new OrgExistsResult(true, null, org);
        }
    }
}
