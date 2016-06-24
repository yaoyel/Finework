using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FineWork.Security.Repos;
using Microsoft.AspNet.Identity;

namespace FineWork.Security.Identity
{
    public static class AspNetIdentityHelper
    {
        public static UserLoginInfo ToLoginInfo(this ILogin login)
        {
            return new UserLoginInfo(login.Provider, login.ProviderKey);
        }

        public static ILogin ToLogin(this UserLoginInfo loginInfo, ILoginRepository loginRepository)
        {
            ILogin login = loginRepository.Create();
            login.Provider = loginInfo.LoginProvider;
            login.ProviderKey = loginInfo.ProviderKey;
            return login;
        }

        /// <summary>
        /// This method is adapted from ApplicationUser.GenerateUserIdentityAsync in the default ASP.NET Identity template.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(IAccount user, UserManager<IAccount, Guid> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}
