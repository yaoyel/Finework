using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;

namespace AppBoot.Data
{
    public static class TransactionTestHelper
    {
        private const String m_ConnectionStringName = "AppBoot";
        private const String m_Sql = "SELECT COUNT(*) FROM master.sys.objects";

        public static ConnectionStringSettings DefaultConnectionStringSettings
        {
            get
            {
                var result = ConfigurationManager.ConnectionStrings[m_ConnectionStringName];
                if (result == null)
                    throw new ConfigurationErrorsException(String.Format(
                        "No configuration file found for ConnectionString [{0}] in: [{1}].",
                        m_ConnectionStringName,
                        AppDomain.CurrentDomain.BaseDirectory));
                return result;
            }
        }

        public static String DefaultConnectionString
        {
            get { return DefaultConnectionStringSettings.ConnectionString; }
        }

        public static void ExecuteSampleCommand()
        {
            using (SqlConnection cn = new SqlConnection())
            {
                cn.ConnectionString = DefaultConnectionString;
                cn.Open();

                using (SqlCommand c = new SqlCommand(m_Sql, cn))
                {
                    c.ExecuteScalar();
                }
            }
        }
    }
}