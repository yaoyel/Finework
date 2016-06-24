using System;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查是否<b>不</b>存在符合条件的 <see cref="StaffEntity"/>. </summary>
    public class StaffNotExistsResult : FineWorkCheckResult
    {
        public StaffNotExistsResult(bool isSucceed, String message, StaffEntity staff)
            : base(isSucceed, message)
        {
            this.Staff = staff;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public StaffEntity Staff { get; private set; }

        public static StaffNotExistsResult Check(IStaffManager staffManager, Guid orgId, Guid accountId)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            var staff = staffManager.FindStaffByOrgAccount(orgId, accountId);
            var message = staff == null ? "" : $"[{staff.Name}]已经是该组织成员";
            return Check(staff, message);
        }

        /// <summary> 检查在一个组织中是否已经存在同名的员工. </summary>
        public static StaffNotExistsResult Check(IStaffManager staffManager, Guid orgId, String staffName)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (String.IsNullOrEmpty(staffName)) throw new ArgumentException("成员名称不能为空.", nameof(staffName));
            var staff = staffManager.FindStaffByNameInOrg(orgId, staffName);
            var message = staff == null ? "" : $"[{staff.Name}]已经是该组织成员";
            return Check(staff, message);
        }

        private static StaffNotExistsResult Check(StaffEntity staff, string message)
        {
            if (staff != null)
            {
                return new StaffNotExistsResult(false, message, staff);
            }
            return new StaffNotExistsResult(true, null, null);
        }
    }
}
