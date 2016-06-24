using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> ��� <see cref="IRole"/> �Ƿ�<b>��</b>����. </summary>
    public class RoleNotExistsResult : CheckResult
    {
        public RoleNotExistsResult(bool isSucceed, String message, IRole role) 
            : base(isSucceed, message)
        {
            this.Role = role;
        }

        /// <summary> �����<b>��</b>ͨ�����������Ӧ�� <see cref="IRole"/>, ����Ϊ <c>null</c>. </summary>
        public IRole Role { get; private set; }

        /// <summary> ���� <see cref="IRole.Name"/> ����Ƿ�<b>��</b>������Ӧ�� <see cref="IRole"/>. </summary>
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