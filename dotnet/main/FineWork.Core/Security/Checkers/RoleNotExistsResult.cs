using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IRole"/> 是否<b>不</b>存在. </summary>
    public class RoleNotExistsResult : CheckResult
    {
        public RoleNotExistsResult(bool isSucceed, String message, IRole role) 
            : base(isSucceed, message)
        {
            this.Role = role;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="IRole"/>, 否则为 <c>null</c>. </summary>
        public IRole Role { get; private set; }

        /// <summary> 根据 <see cref="IRole.Name"/> 检查是否<b>不</b>存在相应的 <see cref="IRole"/>. </summary>
        public static RoleNotExistsResult Check(IRoleManager roleManager, String roleName)
        {
            if (roleManager == null) throw new ArgumentNullException("roleManager");
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentNullException("roleName");

            IRole role = roleManager.FindRoleByName(roleName);
            if (role != null)
            {
                var message = String.Format("Role for name [{0}] exists.", roleName);
                return new RoleNotExistsResult(false, message, role);
            }
            return new RoleNotExistsResult(true, null, null);
        }
    }
}