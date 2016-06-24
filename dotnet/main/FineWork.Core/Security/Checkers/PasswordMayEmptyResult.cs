using System;
using System.Linq;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IAccount.Password"/> 是否可以为 <c>null</c>. </summary>
    /// <remarks> 当 <see cref="IAccount"/> 有一个或多个 <see cref="ILogin"/> 时，
    /// 其 <see cref="IAccount.Password"/> 可以为 <c>null</c>. </remarks>
    public class PasswordMayEmptyResult : CheckResult
    {
        protected PasswordMayEmptyResult(bool isSucceed, String message, IAccount account)
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> 包含被检查的 <see cref="IAccount"/>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> 根据 <see cref="IAccount.Id"/> 返回 <see cref="IAccount.Password"/> 是否可以为 <c>null</c>. </summary>
        /// <returns> 可以为 <c>null</c> 时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PasswordMayEmptyResult Check(IAccountManager accountManager, Guid accountId)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            IAccount account = AccountExistsResult.Check(accountManager, accountId).ThrowIfFailed().Account;
            return Check(accountManager, account);
        }

        /// <summary> 返回 <see cref="IAccount"/> 的 <see cref="IAccount.Password"/> 是否可以为 <c>null</c>. </summary>
        /// <returns> 可以为 <c>null</c> 时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PasswordMayEmptyResult Check(IAccountManager accountManager, IAccount account)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (account == null) throw new ArgumentNullException("account");
            if (account.Logins.Any() == false)
            {
                var message = String.Format("The password is required for user [{0}][{1}] without external logins.",
                    account.Id, account.Name);
                return new PasswordMayEmptyResult(false, message, account);
            }
            return new PasswordMayEmptyResult(true, null, account);
        }
    }
}