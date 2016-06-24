using System;
using System.Security.Claims;
using System.Security.Principal;

namespace AppBoot.Security
{
    public static class ClaimExtensions
    {
        public static String GetNameIdentifier(this IPrincipal principal)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            return GetNameIdentifier(principal.Identity);
        }

        public static String GetNameIdentifier(this IIdentity identity)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            ClaimsIdentity ci = identity as ClaimsIdentity;
            return (ci != null) ? FindFirstValue(ci, ClaimTypes.NameIdentifier) : identity.Name;
        }

        public static String FindFirstValue(this ClaimsIdentity identity, String claimType)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            Claim first = identity.FindFirst(claimType);
            return first != null ? first.Value : null;
        }
    }
}