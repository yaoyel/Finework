using System;
using AppBoot.Checks;
using AppBoot.Security.Passwords;
using FineWork.Security.Passwords;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查密码是否匹配.  </summary>
    public class PasswordMatchResult : CheckResult
    {
        protected PasswordMatchResult(bool isSucceed, String message, IAccount account) 
            : base(isSucceed, message)
        {
            this.Account = account;
        }

        /// <summary> 如检查成功则包含相应的 <see cref="IAccount"/>, 否则为 <c>null</c>. </summary>
        public IAccount Account { get; private set; }

        /// <summary> 检查密码是否匹配. </summary>
        /// <returns> 如匹配则返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PasswordMatchResult Check(IPasswordService passwordService, IAccount account, string password)
        {
            if (account == null) throw new ArgumentNullException("account");

            bool isVerified = passwordService.Verify(account,  password);
            if (isVerified == false)
            {
                var message = String.Format("The password mismatch for user [{0}]([{1}]).", account.Id, account.Name);
                return new PasswordMatchResult(false, message, account);
            }
            return new PasswordMatchResult(true, null, account);
        }

        /// <summary> 检查密码是否可以用于登录。 </summary>
        public static PasswordMatchResult LogOnCheck(IPasswordService passwordService, IAccount account, string password)
        {
            if (account == null) throw new ArgumentNullException("account");

            if (account.PasswordFormat == PasswordFormats.NoPassword)
            {
                return new PasswordMatchResult(false, "The account has no local password set.", account);
            }
            return Check(passwordService, account, password);
        }
    }
}