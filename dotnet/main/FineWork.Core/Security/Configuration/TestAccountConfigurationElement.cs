using System;
using System.Configuration;

namespace FineWork.Security.Configuration
{
    /// <summary> Represents the <see cref="ConfigurationElement"/> for an account, 
    /// typically used in unit tests. </summary>
    public class TestAccountConfigurationElement : ConfigurationElement
    {
        public TestAccountConfigurationElement()
        {
        }

        public TestAccountConfigurationElement(String name)
        {
            this.Name = name;
        }

        private const String m_NamePropertyKey = "name";

        [ConfigurationProperty(m_NamePropertyKey, IsRequired = true, IsKey = true)]
        public String Name
        {
            get { return (String) base[m_NamePropertyKey]; }
            set { base[m_NamePropertyKey] = value; }
        }

        private const String m_ProviderPropertyKey = "provider";

        [ConfigurationProperty(m_ProviderPropertyKey, IsRequired = false)]
        public String Provider
        {
            get { return (String) base[m_ProviderPropertyKey]; }
            set { base[m_ProviderPropertyKey] = value; }
        }

        private const String m_ProviderKeyPropertyKey = "providerKey";

        [ConfigurationProperty(m_ProviderKeyPropertyKey, IsRequired = false)]
        public String ProviderKey
        {
            get { return (String)base[m_ProviderKeyPropertyKey]; }
            set { base[m_ProviderKeyPropertyKey] = value; }
        }

        private const String m_UserNamePropertyKey = "userName";

        [ConfigurationProperty(m_UserNamePropertyKey, IsRequired = true)]
        public String UserName
        {
            get { return (String)base[m_UserNamePropertyKey]; }
            set { base[m_UserNamePropertyKey] = value; }
        }

        private const String m_PasswordPropertyKey = "password";

        [ConfigurationProperty(m_PasswordPropertyKey, IsRequired = true)]
        public String Password
        {
            get { return (String)base[m_PasswordPropertyKey]; }
            set { base[m_PasswordPropertyKey] = value; }
        }
    }
}