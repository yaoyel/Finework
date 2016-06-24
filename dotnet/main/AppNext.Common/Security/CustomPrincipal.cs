using System;
using System.Linq;
using System.Security.Principal;

namespace AppBoot.Security
{
    /// <summary> Represents an <see cref="IPrincipal"/>. </summary>
    /// <remarks> Typically used in unit-tests.</remarks>
    public class CustomPrincipal : IPrincipal
    {
        public CustomPrincipal(IIdentity identity)
            : this(identity, null)
        {
        }

        public CustomPrincipal(IIdentity identity, String[] roles)
        {
            if (identity == null) throw new ArgumentNullException("identity");

            this.m_Identity = identity;
            this.m_Roles = roles;
        }

        private readonly IIdentity m_Identity;

        public IIdentity Identity
        {
            get
            {
                return this.m_Identity;
            }
        }

        private readonly String[] m_Roles;

        public bool IsInRole(String role)
        {
            return (m_Roles != null) && m_Roles.Contains(role);
        }

        public static CustomPrincipal FromIdentity(String identityName, String authenticationType, String[] roles)
        {
            CustomIdentity identity = new CustomIdentity(identityName, authenticationType);
            return new CustomPrincipal(identity, roles);
        }
    }
}