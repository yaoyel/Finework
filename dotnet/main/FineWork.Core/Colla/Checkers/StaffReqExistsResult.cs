using System;
using AppBoot.Checks;
using FineWork.Common;
using System.Collections.Generic;
using System.Linq;

namespace FineWork.Colla.Checkers
{
    /// <summary> 检查是否<b>不</b>存在符合条件的 <see cref="StaffEntity"/>. </summary>
    public class StaffReqExistsResult : FineWorkCheckResult
    {
        public StaffReqExistsResult(bool isSucceed, String message, StaffReqEntity staffReq)
            : base(isSucceed, message)
        {
            this.StaffReq = staffReq;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public StaffReqEntity StaffReq { get; private set; }

        public static StaffReqExistsResult Check(IStaffReqManager staffReqManager, Guid orgId, Guid accountId)
        {
            if (staffReqManager == null) throw new ArgumentNullException(nameof(staffReqManager));
            var staffReq = staffReqManager.FetchStaffReqsByAccount(accountId); 
        
            if (staffReq.Count !=0 && staffReq.Any(p => p.Org.Id == orgId))
                return new StaffReqExistsResult(true, null, staffReq.First(p => p.Org.Id == orgId));
            else
                return new StaffReqExistsResult(false, "不存在对应的组织成员申请信息.", null); 
        } 
    }
}
