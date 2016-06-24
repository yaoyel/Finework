using AppBoot.Checks;
using FineWork.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Checkers
{
    public class PermissionIsAdminResult: FineWorkCheckResult
    {
        private PermissionIsAdminResult(bool isSucceed, String message)
            : base(isSucceed, message)
        {
       
        }

        public static PermissionIsAdminResult Check(IStaffManager staffManager, Guid orgId, Guid accountId)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));

            var staff = staffManager.FindStaffByOrgAccount(orgId, accountId);
            if (staff == null)
            {
                return new PermissionIsAdminResult(false, $"账号 [Id: {accountId}] 不是组织 [Id: {orgId}] 的成员.");
            }
            return Check(staff.Org, staff.Id);
        }

        public static PermissionIsAdminResult Check(IOrgManager orgManager, Guid orgId, Guid staffId)
        {
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            var org = OrgExistsResult.Check(orgManager, orgId).ThrowIfFailed().Org;
            return Check(org, staffId);
        }

        private static PermissionIsAdminResult Check(OrgEntity org, Guid staffId)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));

            if (org.AdminStaff.Id != staffId)
            {
                return new PermissionIsAdminResult(false, $"员工 [Id: {staffId}] 不是组织 [{org.Name}] 的管理员.");
            }
            return new PermissionIsAdminResult(true, null);
        }
    }
}
