using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class AccessTimeExistsResult:FineWorkCheckResult
    {
        public AccessTimeExistsResult(bool isSucceed, String message, AccessTimeEntity accessTime)
            : base(isSucceed, message)
        {
            this.AccessTime = accessTime;
        }

        public AccessTimeEntity AccessTime { get; set; }
        public static AccessTimeExistsResult Check(IAccessTimeManager accessTimeManager, Guid accessTimeId)
        {
            AccessTimeEntity accessTime = accessTimeManager.FindAccessTimeById(accessTimeId);
            return Check(accessTime, null);
        }

        public static AccessTimeExistsResult CheckByStaff(IAccessTimeManager accessTimeManager, Guid staffId)
        {
            var accessTime = accessTimeManager.FindAccessTimeByStaffId(staffId);
            return Check(accessTime, null);
        }

        private static AccessTimeExistsResult Check([CanBeNull]  AccessTimeEntity accessTime, String message)
        {
            if (accessTime == null)
            {
                return new AccessTimeExistsResult(false, message, null);
            }
         
            return new AccessTimeExistsResult(true, null, accessTime);
        }
    }
}
