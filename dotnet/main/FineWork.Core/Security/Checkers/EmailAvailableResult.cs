using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using FineWork.Common;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="IAccount.Email"/> 是否为新账号使用. </summary>
    public class EmailAvailableResult : CheckResult
    {
        private EmailAvailableResult(bool isSucceed, String message, IEnumerable<IAccount> conflicts)
            : base(isSucceed, message)
        {
            this.Conflicts = conflicts;
        }

        /// <summary> 若检查失败, 则包含相应的 <see cref="IAccount"/> 集合, 否则为<c>null</c>. </summary>
        public IEnumerable<IAccount> Conflicts { get; private set; }

        /// <summary> 检查 <paramref name="email"/> 是否可以作为新账号的 <see cref="IAccount.Email"/>. </summary>
        /// <param name="accountManager">The <see cref="IAccountManager"/></param>
        /// <param name="email">The email address to be checked.</param>
        /// <param name="allowReuse">If an email address can be registered by multiple users.</param>
        public static EmailAvailableResult Check(IAccountManager accountManager, String email, bool allowReuse)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("email is null or empty.", "email");

            if (!allowReuse)
            {
                var users = accountManager.FetchAccountsByEmail(email);
                if (users.Any())
                {
                    String errorMessage = String.Format("Email [{0}] is already registered.", email);
                    return new EmailAvailableResult(false, errorMessage, users);
                }
            }
            return new EmailAvailableResult(true, null, null);
        }

        public override Exception CreateException(string message)
        {
            return new FineWorkException(message);
        }
    }
}