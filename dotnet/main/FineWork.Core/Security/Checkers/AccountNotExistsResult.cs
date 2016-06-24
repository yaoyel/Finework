using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> ��� <see cref="IAccount"/> �Ƿ�<b>��</b>����. </summary>
    public class AccountNotExistsResult : CheckResult
    {
        public AccountNotExistsResult(bool isSucceed, String message, IAccount account) 
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> �����<b>��</b>ͨ�����������Ӧ�� <see cref="IAccount"/>, ����Ϊ <c>null</c>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> ���� <see cref="IAccount.Name"/> ����Ƿ�<b>��</b>������Ӧ�� <see cref="IAccount"/>. </summary>
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