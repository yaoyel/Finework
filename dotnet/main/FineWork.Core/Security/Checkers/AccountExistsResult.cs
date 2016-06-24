using System;
using AppBoot.Checks;
using JetBrains.Annotations;

namespace FineWork.Security.Checkers
{
    /// <summary> ��� <see cref="IAccount"/> �Ƿ����. </summary>
    public class AccountExistsResult : CheckResult
    {
        public AccountExistsResult(bool isSucceed, String message, IAccount account)
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> �����ͨ�����������Ӧ�� <see cref="IAccount"/>, ����Ϊ <c>null</c>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> ���� <see cref="IAccount.Id"/> �����Ƿ������Ӧ�� <see cref="IAccount"/>. </summary>
        /// <returns> ����ʱ���� <c>true</c>, ������ʱ���� <c>false</c>. </returns>
        public static AccountExistsResult Check(IAccountManager accountManager, Guid accountId)
        {
            IAccount account = accountManager.FindAccount(accountId);
            return Check(account, String.Format("Invalid account Id [{0}].", accountId));
        }

        /// <summary> ���� <see cref="IAccount.Name"/> �����Ƿ������Ӧ�� <see cref="IAccount"/>. </summary>
        /// <returns> ����ʱ���� <c>true</c>, ������ʱ���� <c>false</c>. </returns>
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