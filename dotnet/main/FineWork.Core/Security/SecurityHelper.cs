using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using AppBoot.Common;
using AppBoot.Security;

namespace FineWork.Security
{
    public class FwClaimTypes
    {
        public const String AccountId = "FineWork_ClaimType_AccountId";

        public const String AccountName = "FineWork_ClaimType_AccountName";
    }

    public static class SecurityHelper
    {
        private const String m_IdentityProviderType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        private static readonly String m_DefaultGuidFormat = GuidFormats.HyphenDigits.GetFormatString();

        /// <summary> 从 <see cref="IAccount"/> 创建 <see cref="ClaimsIdentity"/>. </summary>
        public static ClaimsIdentity CreateIdentity(IAccount account, String authenticationType)
        {
            if (account == null) throw new ArgumentNullException("account");
            
            ClaimsIdentity identity = new ClaimsIdentity(authenticationType, ClaimTypes.Name, ClaimTypes.Role);

            var accountId = account.Id.ToString(m_DefaultGuidFormat);

            //加入标准的 claim, 以代 ASP.NET Identity 等使用.
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, accountId, ClaimValueTypes.String));
            identity.AddClaim(new Claim(ClaimTypes.Name, account.Name??"", ClaimValueTypes.String));
            identity.AddClaim(new Claim(m_IdentityProviderType, "ASP.NET Identity", ClaimValueTypes.String));

            //加入 FineWork 特定的 claim
            identity.AddClaim(new Claim(FwClaimTypes.AccountId, accountId, ClaimValueTypes.String));
            identity.AddClaim(new Claim(FwClaimTypes.AccountName, account.Name??""));

            return identity;
        }

        #region GetAccountId/FindAccountId

        /// <summary> 获取相关联的 <see cref="IAccount.Id"/>. </summary>
        /// <exception cref="AuthenticationException"> 若不存在相应的 <see cref="IAccount.Id"/>. </exception>
        public static Guid GetAccountId(this IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var claim = FindFirstClaim(principal, AccountIdClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return Guid.Parse(claim.Value);
            }
            throw new AuthenticationException("The user has not been authencated, or has no associated account.");
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Id"/>. </summary>
        /// <returns> 若存在则返回相应的 <see cref="IAccount.Id"/>, 否则返回 <c>null</c>. </returns>
        public static Guid? FindAccountId(this IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var claim = FindFirstClaim(principal, AccountIdClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return Guid.Parse(claim.Value);
            }
            return null;
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Id"/>. </summary>
        /// <exception cref="AuthenticationException"> 若不存在相应的 <see cref="IAccount.Id"/>. </exception>
        public static Guid GetAccountId(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var claim = FindFirstClaim(identity, AccountIdClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return Guid.Parse(claim.Value);
            }
            throw new AuthenticationException("The user has not been authencated, or has no associated account.");
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Id"/>. </summary>
        /// <returns> 若存在则返回相应的 <see cref="IAccount.Id"/>, 否则返回 <c>null</c>. </returns>
        public static Guid? FindAccountId(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var claim = FindFirstClaim(identity, AccountIdClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return Guid.Parse(claim.Value);
            }
            return null;
        }

        #endregion

        #region GetAccountName/FindAccountName

        /// <summary> 获取相关联的 <see cref="IAccount.Name"/>. </summary>
        /// <exception cref="AuthenticationException"> 若不存在相应的 <see cref="IAccount.Name"/>. </exception>
        public static String GetAccountName(this IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var claim = FindFirstClaim(principal, AccountNameClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return claim.Value;
            }
            throw new AuthenticationException("The user has not been authencated, or has no associated account.");
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Name"/>. </summary>
        /// <returns> 若存在则返回相应的 <see cref="IAccount.Name"/>, 否则返回 <c>null</c>. </returns>
        public static String FindAccountName(this IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var claim = FindFirstClaim(principal, AccountNameClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return claim.Value;
            }
            return null;
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Name"/>. </summary>
        /// <exception cref="AuthenticationException"> 若不存在相应的 <see cref="IAccount.Name"/>. </exception>
        public static String GetAccountName(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var claim = FindFirstClaim(identity, AccountNameClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return claim.Value;
            }
            throw new AuthenticationException("The user has not been authencated, or has no associated account.");
        }

        /// <summary> 获取相关联的 <see cref="IAccount.Name"/>. </summary>
        /// <returns> 若存在则返回相应的 <see cref="IAccount.Name"/>, 否则返回 <c>null</c>. </returns>
        public static String FindAccountName(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var claim = FindFirstClaim(identity, AccountIdClaimMatcher);
            if (claim != null && claim.Value != null)
            {
                return claim.Value;
            }
            return null;
        }

        #endregion

        private static bool AccountIdClaimMatcher(Claim claim)
        {
            return claim.Type == FwClaimTypes.AccountId;
        }

        private static bool AccountNameClaimMatcher(Claim claim)
        {
            return claim.Type == FwClaimTypes.AccountName;
        }

        private static Claim FindFirstClaim(IPrincipal principal, Predicate<Claim> matcher)
        {
            if (principal == null) return null;
            if (principal.Identity == null) return null;
            if (principal.Identity.IsAuthenticated == false) return null;

            return FindFirstClaim(principal.Identity, matcher);
        }

        private static Claim FindFirstClaim(IIdentity identity, Predicate<Claim> matcher)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity == null) return null;

            var claim = claimsIdentity.FindFirst(matcher);
            return claim;
        }

        private static ClaimsIdentity RemoveAll(this ClaimsIdentity identity, Predicate<Claim> matcher)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (matcher == null) throw new ArgumentNullException(nameof(matcher));

            var existedClaims = identity.FindAll(matcher);
            foreach (var existed in existedClaims)
            {
                identity.RemoveClaim(existed);
            }
            return identity;
        }
    }
}
