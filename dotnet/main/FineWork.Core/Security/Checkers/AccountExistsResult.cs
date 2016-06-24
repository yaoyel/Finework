using System;
using AppBoot.Checks;
using JetBrains.Annotations;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IAccount"/> 是否存在. </summary>
    public class AccountExistsResult : CheckResult
    {
        public AccountExistsResult(bool isSucceed, String message, IAccount account)
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="IAccount"/>, 否则为 <c>null</c>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> 根据 <see cref="IAccount.Id"/> 返回是否存在相应的 <see cref="IAccount"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static AccountExistsResult Check(IAccountManager accountManager, Guid accountId)
        {
            IAccount account = accountManager.FindAccount(accountId);
            return Check(account, String.Format("Invalid account Id [{0}].", accountId));
        }

        /// <summary> 根据 <see cref="IAccount.Name"/> 返回是否存在相应的 <see cref="IAccount"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static AccountExistsResult Check(IAccountManager accountManager, String accountName)
        {
            IAccount account = accountManager.FindAccountByName(accountName);
            return Check(account, String.Format("Invalid account name [{0}].", accountName));
        }

        public static AccountExistsResult CheckByPhoneNumber(IAccountManager accountManager, string phoneNumber)
        {
            IAccount account = accountManager.FindAccountByPhoneNumber(phoneNumber);
            return Check(account, String.Format("Invalid account name [{0}].", phoneNumber));
        }

        private static AccountExistsResult Check([CanBeNull] IAccount account, String message)
        {
            if (account == null)
            {
                return new AccountExistsResult(false, message, null);
            }
            return new AccountExistsResult(true, null, account);
        }
    }
}