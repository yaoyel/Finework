using System;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查 <see cref="ILogin"/> 是否 <b>不</b>存在. </summary>
    public class LoginNotExistsResult : CheckResult
    {
        private LoginNotExistsResult(bool isSucceed, String message, ILogin existedLogin)
            : base(isSucceed, message)
        {
            this.ExistedLogin = existedLogin;
        }

        /// <summary> 若检查失败则包含已存在的 <see cref="ILogin"/>, 否则为 <c>null</c>. </summary>
        public ILogin ExistedLogin { get; private set; }

        /// <summary> 根据 <see cref="ILogin.Provider"/> 与 <see cref="ILogin.ProviderKey"/> 检查是否<b>不</b>存在相应的 <see cref="ILogin"/>. </summary>
        /// <returns> 若<b>不</b>存在则返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static LoginNotExistsResult Check(IAccountManager accountManager, String provider, String providerKey)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");
            if (String.IsNullOrEmpty(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrEmpty(providerKey)) throw new ArgumentNullException("providerKey");

            var login = accountManager.FindLogin(provider, providerKey);
            if (login != null)
            {
                var message = String.Format("Login for provider [{0}] with key [{1}] exists.", provider, providerKey);
                return new LoginNotExistsResult(false, message, login);
            }
            return new LoginNotExistsResult(true, null, null);
        }
    }
}