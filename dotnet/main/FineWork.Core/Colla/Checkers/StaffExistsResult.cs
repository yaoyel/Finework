using System;
using AppBoot.Checks;
using FineWork.Security;
using JetBrains.Annotations;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查 <see cref="StaffEntity"/> 是否存在. </summary>
    public class StaffExistsResult : FineWorkCheckResult
    {
        public StaffExistsResult(bool isSucceed, String message, StaffEntity staff)
            : base(isSucceed, message)
        {
            this.Staff = staff;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public StaffEntity Staff { get; private set; }

        /// <summary> 根据 <see cref="StaffEntity.Id"/> 检查 <see cref="StaffEntity"/> 是否存在. </summary>
        /// <returns> <c>true</c> 表示存在， <c>false</c> 表示不存在. </returns>
        public static StaffExistsResult Check(IStaffManager staffManager, Guid staffId, bool checkIsEnabled = true)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            var staff = staffManager.FindStaff(staffId);
            return Check(staff, "不存在对应的员工信息.",checkIsEnabled);
        }

        /// <summary> 检查 <see cref="OrgEntity.Id"/> 与 <see cref="IAccount.Id"/> 检查 <see cref="StaffEntity"/>是否存在. </summary>
        /// <returns> <c>true</c> 表示存在， <c>false</c> 表示不存在. </returns>
        public static StaffExistsResult Check(IStaffManager staffManager, Guid orgId, Guid accountId, bool checkIsEnabled = true)
        {
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            var staff = staffManager.FindStaffByOrgAccount(orgId, accountId);
            return Check(staff, "不存在对应的员工信息.",checkIsEnabled);
        }

        public static StaffExistsResult CheckForAdmin(IStaffManager staffManager, Guid orgId, Guid accountId, bool checkIsEnabled = true) 
        {  
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            
            var staff = Check(staffManager, orgId, accountId).ThrowIfFailed().Staff;
            if(staff.Org.AdminStaff==staff)
                return new StaffExistsResult(true, null, staff);
            else
                return new StaffExistsResult(false, "你不是当前组织的管理员.", null);

        }

        private static StaffExistsResult Check([CanBeNull] StaffEntity staff, String message,bool checkIsEnabled=true)
        {
            if (staff == null)
            {
                return new StaffExistsResult(false, message, null);
            }

            if (!staff.IsEnabled && checkIsEnabled)
                return new StaffExistsResult(false, "该员工已被当前组织禁用，请重新选择组织.", staff);

            return new StaffExistsResult(true, null, staff);
          
        }
    }
}