using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> ��� <see cref="ILogin"/> �Ƿ���Ա�ɾ��. </summary>
    /// <remarks> �� <see cref="IAccount"/> �б��ص� <see cref="IAccount.Password"/> ���߻��������� <see cref="ILogin"/>
    /// ����������֤�û����ʱ <see cref="ILogin"/> ���Բ�ɾ��. </remarks>
    public class LoginRemovableResult : CheckResult
    {
        public LoginRemovableResult(bool isSucceed, String message, ILogin login) 
            : base(isSucceed, message)
        {
            this.Login = login;
        }

        /// <summary> ���������� <see cref="ILogin"/>. </summary>
        /// <remarks> ���ۼ��ɹ���ʧ��, ��ֵ����Ϊ <c>null</c>. </remarks>
        public ILogin Login { get; private set; }

        /// <summary> ���� <see cref="ILogin.Id"/> ��� <see cref="ILogin"/> �Ƿ���Ա�ɾ��. </summary>
        /// <returns> ���Ա�ɾ��ʱ���� <c>true</c>, ���򷵻� <c>false</c>. </returns>
        public static LoginRemovableResult Check(IAccountManager accountManager, Guid loginId)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");

            var login = LoginExistsResult.Check(accountManager, loginId).ThrowIfFailed().Login;
            return Check(accountManager, login);
        }

        private const String m_LastLoginErrorFmt =
            "The login with Id [{0}] cannot be removed because it is the last login and its user has no local password.";

        /// <summary> ��� <see cref="ILogin"/> �Ƿ���Ա�ɾ��. </summary>
        /// <returns> ���Ա�ɾ��ʱ���� <c>true</c>, ���򷵻� <c>false</c>. </returns>
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