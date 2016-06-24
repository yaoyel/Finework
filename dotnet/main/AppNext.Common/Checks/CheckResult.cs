using System;

namespace AppBoot.Checks
{
    /// <summary> Represents the result of rule checking. </summary>
    public class CheckResult : ICheckResult
    {
        public CheckResult(bool isSucceed, String message)
        {
            this.IsSucceed = isSucceed;
            this.Message = message;
        }

        /// <summary> Indicates if the check succeed. </summary>
        public bool IsSucceed { get; private set; }

        /// <summary> Gets the error message. </summary>
        public String Message { get; private set; }

        public virtual Exception CreateException(String message)
        {
            return new ApplicationException(message);
        }
    }
}