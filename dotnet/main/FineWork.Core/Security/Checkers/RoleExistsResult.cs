using System;
using AppBoot.Checks;
using JetBrains.Annotations;

namespace FineWork.Security.Checkers
{
    /// <summary> ��� <see cref="IRole"/> �Ƿ����. </summary>
    public class RoleExistsResult : CheckResult
    {
        public RoleExistsResult(bool isSucceed, String message, IRole role)
            : base(isSucceed, message)
        {
            this.Role = role;
        }

        /// <summary> �����ͨ�����������Ӧ�� <see cref="IRole"/>, ����Ϊ <c>null</c>. </summary>
        public IRole Role { get; private set; }

        /// <summary> ���� <see cref="IRole.Id"/> �����Ƿ������Ӧ�� <see cref="IRole"/>. </summary>
        /// <returns> ����ʱ���� <c>true</c>, ������ʱ���� <c>false</c>. </returns>
        public static RoleExistsResult Check(IRoleManager roleManager, Guid roleId)
        {
            IRole account = roleManager.FindRole(roleId);
            return Check(account, String.Format("Invalid role Id [{0}].", roleId));
        }

        /// <summary> ���� <see cref="IRole.Name"/> �����Ƿ������Ӧ�� <see cref="IRole"/>. </summary>
        /// <returns> ����ʱ���� <c>true</c>, ������ʱ���� <c>false</c>. </returns>
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