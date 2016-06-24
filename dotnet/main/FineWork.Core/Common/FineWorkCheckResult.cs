using System;
using AppBoot.Checks;

namespace FineWork.Common
{
    public class FineWorkCheckResult : CheckResult, IFriendlyMessage
    {
        public FineWorkCheckResult(bool isSucceed, string message)
            : base(isSucceed, message)
        {
        }

        public String FriendlyMessage { get; set; }

        public override Exception CreateException(string message)
        {
            FineWorkException exception = new FineWorkException(message);
            exception.FriendlyMessage = this.FriendlyMessage;
            return exception;
        }
    }
}
