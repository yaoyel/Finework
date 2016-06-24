using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;

namespace AppBoot.TestCommon
{
    public static class SqlTestUtil
    {
        /// <summary>
        /// Checks if the connection string for a name exists.
        /// </summary>
        /// <param name="connectionStringName"></param>
        public static String GetConnectionStringFromName(String connectionStringName)
        {
            if (connectionStringName == null) throw new ArgumentNullException("connectionStringName");
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
            {
                throw new ArgumentException(String.Format("Cannot find connection string with name : [{0}]", "connectionStringName"));
            }
            return cs.ConnectionString;
        }

        /// <summary>
        /// Checks if the <paramref name="connectionString"/> can be connected.
        /// </summary>
        /// <param name="connectionString"></param>
        public static void CheckConnectionStringIsConnectable(String connectionString)
        {
            if (String.IsNullOrEmpty(connectionString)) throw new ArgumentException("connectionString is null or empty.", "connectionString");

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT 1", cn))
                {
                    var dr = cmd.ExecuteReader();
                    var hasRecord = dr.Read();
                    if (!hasRecord) Assert.Fail("The SqlDataReader has no record returned.");
                    var actual = (int)dr[0];
                    Assert.AreEqual(1, actual);
                }
            }
        }
    }
}
