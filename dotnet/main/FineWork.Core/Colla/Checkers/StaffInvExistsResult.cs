using System;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查是否<b>不</b>存在符合条件的 <see cref="StaffEntity"/>. </summary>
    public class StaffInvExistsResult : FineWorkCheckResult
    {
        public StaffInvExistsResult(bool isSucceed, String message, StaffInvEntity staffInv)
            : base(isSucceed, message)
        {
            this.StaffInv = staffInv;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public StaffInvEntity StaffInv { get; private set; }

        public static StaffInvExistsResult Check(IStaffInvManager staffInvManager, Guid orgId, Guid accountId)
        {
            if (staffInvManager == null) throw new ArgumentNullException(nameof(staffInvManager));
            var staffInv = staffInvManager.FindStaffInvByOrgAccount(orgId, accountId);
            return Check(staffInv, "不存在对应的组织成员邀请信息.");
        }


        public static StaffInvExistsResult CheckByPhoneNumber(IStaffInvManager staffInvManager, Guid orgId,string phoneNumber)
        {
            if (staffInvManager == null) throw new ArgumentNullException(nameof(staffInvManager));
            var staffInv = staffInvManager.FindStaffInvByOrgWithPhoneNumber(orgId, phoneNumber);
            return Check(staffInv, "不存在对应的组织成员邀请信息.");
        }


        private static StaffInvExistsResult Check(StaffInvEntity staff, string message)
        {
            if (staff == null)
            {
                return new StaffInvExistsResult(false, message, null);
            }
            return new StaffInvExistsResult(true, null, staff);
        }
    }
}
