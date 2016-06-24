using System;
using System.Security.Principal;

namespace AppBoot.Security
{
    /// <summary> Represents an <see cref="IIdentity"/>. </summary>
    /// <remarks> Typically used in unit-tests.</remarks>
    public class CustomIdentity : IIdentity
    {
        public CustomIdentity(String name, String authenticationType)
        {
            this.m_Name = name;
            this.m_AuthenticationType = authenticationType;
        }

        private readonly String m_Name;

        public String Name
        {
            get { return this.m_Name; }
        }
        
        private readonly String m_AuthenticationType;

        public String AuthenticationType
        {
            get { return m_AuthenticationType; }
        }

        public bool IsAuthenticated
        {
            get { return !String.IsNullOrEmpty(this.m_Name); }
        }
    }
}