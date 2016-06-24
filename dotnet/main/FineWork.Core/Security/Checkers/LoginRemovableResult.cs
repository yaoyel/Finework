using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="ILogin"/> 是否可以被删除. </summary>
    /// <remarks> 当 <see cref="IAccount"/> 有本地的 <see cref="IAccount.Password"/> 或者还有其他的 <see cref="ILogin"/>
    /// 可以用于验证用户身份时 <see cref="ILogin"/> 可以补删除. </remarks>
    public class LoginRemovableResult : CheckResult
    {
        public LoginRemovableResult(bool isSucceed, String message, ILogin login) 
            : base(isSucceed, message)
        {
            this.Login = login;
        }

        /// <summary> 包含被检查的 <see cref="ILogin"/>. </summary>
        /// <remarks> 无论检查成功或失败, 该值都不为 <c>null</c>. </remarks>
        public ILogin Login { get; private set; }

        /// <summary> 根据 <see cref="ILogin.Id"/> 检查 <see cref="ILogin"/> 是否可以被删除. </summary>
        /// <returns> 可以被删除时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static LoginRemovableResult Check(IAccountManager accountManager, Guid loginId)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");

            var login = LoginExistsResult.Check(accountManager, loginId).ThrowIfFailed().Login;
            return Check(accountManager, login);
        }

        private const String m_LastLoginErrorFmt =
            "The login with Id [{0}] cannot be removed because it is the last login and its user has no local password.";

        /// <summary> 检查 <see cref="ILogin"/> 是否可以被删除. </summary>
        /// <returns> 可以被删除时返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static LoginRemovableResult Check(IAccountManager accountManager, ILogin login)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (login == null) throw new ArgumentNullException("login");

            var account = login.Account;
            if (account.Logins.Count == 1)
            {
                if (String.IsNullOrEmpty(account.Password))
                {
                    String message = String.Format(m_LastLoginErrorFmt, login.Id);
                    return new LoginRemovableResult(false, message, login);
                }
            }
            return new LoginRemovableResult(true, null, login);
        }
    }
}