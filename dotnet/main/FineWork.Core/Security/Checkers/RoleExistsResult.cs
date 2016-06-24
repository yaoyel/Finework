using System;
using AppBoot.Checks;
using JetBrains.Annotations;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IRole"/> 是否存在. </summary>
    public class RoleExistsResult : CheckResult
    {
        public RoleExistsResult(bool isSucceed, String message, IRole role)
            : base(isSucceed, message)
        {
            this.Role = role;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="IRole"/>, 否则为 <c>null</c>. </summary>
        public IRole Role { get; private set; }

        /// <summary> 根据 <see cref="IRole.Id"/> 返回是否存在相应的 <see cref="IRole"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static RoleExistsResult Check(IRoleManager roleManager, Guid roleId)
        {
            IRole account = roleManager.FindRole(roleId);
            return Check(account, String.Format("Invalid role Id [{0}].", roleId));
        }

        /// <summary> 根据 <see cref="IRole.Name"/> 返回是否存在相应的 <see cref="IRole"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static RoleExistsResult Check(IRoleManager roleManager, String roleName)
        {
            IRole account = roleManager.FindRoleByName(roleName);
            return Check(account, String.Format("Invalid role name [{0}].", roleName));
        }

        private static RoleExistsResult Check([CanBeNull] IRole account, String message)
        {
            if (account == null)
            {
                return new RoleExistsResult(false, message, null);
            }
            return new RoleExistsResult(true, null, account);
        }
    }
}