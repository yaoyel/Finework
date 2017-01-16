using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class PushLogExistsResult : FineWorkCheckResult
    {
        public PushLogExistsResult(bool isSucceed, String message, PushLogEntity pushLog)
            : base(isSucceed, message)
        {
            this.PushLog = pushLog;
        }

        public PushLogEntity PushLog { get; set; }
        public static PushLogExistsResult Check(IPushLogManager pushLogManager, Guid pushLogId)
        {
            var pushLog = pushLogManager.FindById(pushLogId);
            return Check(pushLog, "该记录不存在");
        }
 
        private static PushLogExistsResult Check([CanBeNull]  PushLogEntity pushLog, String message)
        {
            if (pushLog== null)
            {
                return new PushLogExistsResult(false, message, null);
            }

            return new PushLogExistsResult(true, null, pushLog);
        }
    }
}
