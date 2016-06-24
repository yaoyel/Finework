using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="ILogin"/> 是否存在. </summary>
    public class LoginExistsResult : CheckResult
    {
        public LoginExistsResult(bool isSucceed, String message, ILogin login)
            : base(isSucceed, message)
        {
            this.Login = login;
        }

        /// <summary> 若检查通过，则包含相应的 <see cref="ILogin"/>, 否则为 <c>null</c>. </summary>
        public ILogin Login { get; private set; }

        /// <summary> 根据 <see cref="ILogin.Id"/> 返回是否存在相应的 <see cref="ILogin"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static LoginExistsResult Check(IAccountManager accountManager, Guid loginId)
        {
            ILogin login = accountManager.FindLogin(loginId);
            if (login == null)
            {
                var message = String.Format("Invalid loginId Id [{0}].", loginId);
                return new LoginExistsResult(false, message, null);
            }
            return new LoginExistsResult(true, null, login);
        }

        /// <summary> 根据 <see cref="ILogin.Provider"/> 与 <see cref="ILogin.ProviderKey"/> 返回是否存在相应的 <see cref="ILogin"/>. </summary>
        /// <returns> 存在时返回 <c>true</c>, 不存在时返回 <c>false</c>. </returns>
        public static LoginExistsResult Check(IAccountManager accountManager, String provider, String providerKey)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");

            ILogin login = accountManager.FindLogin(provider, providerKey);
            if (login == null)
            {
                var message = String.Format("Invalid login for provider [{0}] with Key [{1}].", provider, providerKey);
                return new LoginExistsResult(false, message, null);
            }
            return new LoginExistsResult(true, null, login);
        }
    }
}