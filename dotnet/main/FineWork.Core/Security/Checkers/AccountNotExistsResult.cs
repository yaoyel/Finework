using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IAccount"/> 是否<b>不</b>存在. </summary>
    public class AccountNotExistsResult : CheckResult
    {
        public AccountNotExistsResult(bool isSucceed, String message, IAccount account) 
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="IAccount"/>, 否则为 <c>null</c>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> 根据 <see cref="IAccount.Name"/> 检查是否<b>不</b>存在相应的 <see cref="IAccount"/>. </summary>
        public static AccountNotExistsResult Check(IAccountManager accountManager, String accountName)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (String.IsNullOrEmpty(accountName)) throw new ArgumentNullException("accountName");

            IAccount account = accountManager.FindAccountByName(accountName);
            if (account != null)
            {  
                var message = String.Format("Account for name [{0}] exists.", accountName);
                return new AccountNotExistsResult(false, message, account);
            }
            return new AccountNotExistsResult(true, null, null);
        }
    }
}