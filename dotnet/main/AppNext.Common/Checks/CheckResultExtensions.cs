using System;

namespace AppBoot.Checks
{
    public static class CheckResultExtensions
    {
        public static T ThrowIfFailed<T>(this T checkResult) where T : class, ICheckResult
        {
            if (checkResult == null) throw new ArgumentNullException("checkResult");
            if (!checkResult.IsSucceed)
            {
                throw checkResult.CreateException(checkResult.Message ?? String.Empty);
            }
            return checkResult;
        }
    }
}