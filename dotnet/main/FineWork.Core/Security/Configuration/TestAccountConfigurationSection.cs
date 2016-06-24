using System;
using System.Configuration;

namespace FineWork.Security.Configuration
{
    /// <summary> Represents the <see cref="ConfigurationSection"/> for accounts, 
    /// typically used in unit tests. </summary>
    /// <remarks> This section typically stored externally to <c>app.config</c>:
    /// <example>
    /// <![CDATA[
    /// <configSections>
    ///     <section name="testAccounts" type="FineWork.Security.Configuration.TestAccountConfigurationSection, FineWork.Core"/>
    /// </configSections>
    /// <testAccounts configSource="Configs\testAccounts.config" />
    /// ]]>
    /// </example>
    /// </remarks>
    public class TestAccountConfigurationSection : ConfigurationSection
    {
        private const String m_AccountsPropertyKey = "accounts";

        [ConfigurationProperty(m_AccountsPropertyKey, IsDefaultCollection = true)]
        public TestAccountConfigurationElementCollection Accounts
        {
            get { return (TestAccountConfigurationElementCollection) base[m_AccountsPropertyKey]; }
        }
    }
}
