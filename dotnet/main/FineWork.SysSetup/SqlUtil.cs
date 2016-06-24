using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Data.Aef;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace FineWork.SysSetup
{
    public static class SqlUtil
    {
        public static String ChangeToMaster(String connectionString)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            return ChangeDatabase(connectionString, "master");
        }

        public static String ChangeDatabase(String connectionString, String databaseName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseName, nameof(databaseName));

            var csBuilder = new SqlConnectionStringBuilder(connectionString);
            csBuilder.InitialCatalog = databaseName;
            return csBuilder.ToString();
        }

        /// <summary> 判断数据库是否存在. </summary>
        /// <param name="connectionString"> 连接字符串. </param>
        /// <param name="databaseName"> 数据库名称. </param>
        /// <remarks>
        /// <paramref name="connectionString"/> 的 <c>Inital Catalog</c> 不应为 <paramref name="databaseName"/>, 否则当数据库不存在时会抛出异常.
        /// </remarks>
        public static bool IsDatabaseExists(String connectionString, String databaseName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseName, nameof(databaseName));

            var cs = ChangeToMaster(connectionString);
            var sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
            var count = ExecuteScalar<int>(cs, sql);
            return count > 0;
        }

        /// <summary> 判断数据库是否存在. </summary>
        /// <param name="connectionString"> 连接字符串. </param>
        /// <param name="databaseDir"> 数据库文件所在的目录. </param>
        /// <param name="databaseName"> 数据库名称. </param>
        /// <remarks>
        /// <paramref name="connectionString"/> 的 <c>Inital Catalog</c> 不应为 <paramref name="databaseName"/>, 否则当数据库不存在时会抛出异常.
        /// </remarks>
        public static void CreateDatabase(String connectionString, String databaseDir, String databaseName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseDir, nameof(databaseDir));
            Args.NotEmpty(databaseName, nameof(databaseName));

            /* Cannot use Directory.Exists when the connectionString represents a remote server. */
            /* 
            if (Directory.Exists(databaseDir) == false)
            {
                throw new ArgumentException($"The directory {databaseDir} does not exist.");
            }
            */
            bool dbExists = SqlUtil.IsDatabaseExists(connectionString, databaseName);
            if (dbExists)
            {
                throw new InvalidOperationException($"Cannot create databae [{databaseName}] because it exists already.");
            }

            var dbFileName = Path.Combine(databaseDir, databaseName);
            var logFileName = Path.Combine(databaseDir, databaseName + "_Log");

            var sql =
                $@"CREATE DATABASE [{databaseName}] 
                    ON  PRIMARY ( 
                        NAME = N'{databaseName}', 
                        FILENAME = N'{dbFileName}.mdf', 
                        SIZE = 10240KB , 
                        FILEGROWTH = 1024KB 
                    )
	                LOG ON ( 
                        NAME = N'{databaseName}_Log', 
                        FILENAME = N'{logFileName}.ldf', 
                        SIZE = 5120KB , 
                        FILEGROWTH = 10%)
	                COLLATE Chinese_PRC_CI_AS;";
            SqlUtil.ExecuteNonQuery(connectionString, sql);
        }


        /// <summary> 删除数据库. </summary>
        /// <param name="connectionString"> 连接字符串.</param>
        /// <param name="databaseName"> 数据库名称. </param>
        public static void DropDatabase(String connectionString, String databaseName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseName, nameof(databaseName));

            bool dbExists = SqlUtil.IsDatabaseExists(connectionString, databaseName);
            if (!dbExists)
            {
                throw new InvalidOperationException($"Cannot drop databae [{databaseName}] because it does not exist.");
            }

            String sql =
                $@"ALTER DATABASE {databaseName} SET OFFLINE WITH ROLLBACK IMMEDIATE;
	                ALTER DATABASE {databaseName} SET ONLINE;
	                DROP DATABASE {databaseName};";

            ExecuteNonQuery(connectionString, sql);
        }

        public static bool IsLoginExists(String connectionString, String loginName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(loginName, nameof(loginName));

            var sql = $"SELECT COUNT(*) FROM master.sys.syslogins WHERE name = '{loginName}'";
            var count = ExecuteScalar<int>(connectionString, sql);
            return count > 0;
        }

        /// <summary> 创建数据库服务器的 <c>login</c>. </summary>
        /// <param name="connectionString"> 连接字符串.</param>
        /// <param name="loginName"> login 的名称. </param>
        /// <remarks> 本方法目前只能用于映射 <c>Windows</c> 系统账号. </remarks>
        public static void CreateLogin(String connectionString, String loginName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(loginName, nameof(loginName));

            bool exists = SqlUtil.IsLoginExists(connectionString, loginName);
            if (exists)
            {
                throw new InvalidOperationException($"Cannot create login [{loginName}] because it exists already.");
            }

            var sql = $"CREATE LOGIN [{loginName}] FROM WINDOWS";
            SqlUtil.ExecuteNonQuery(connectionString, sql);
        }

        public static bool IsUserExists(String connectionString, String databaseName, String userName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseName, nameof(databaseName));
            Args.NotEmpty(userName, nameof(userName));

            var sql = $"SELECT COUNT(*) FROM {databaseName}.dbo.sysusers WHERE name = '{userName}'";
            var count = ExecuteScalar<int>(connectionString, sql);
            return count > 0;
        }

        /// <summary> 创建数据库用户. </summary>
        /// <param name="connectionString"> 连接字符串.</param>
        /// <param name="databaseName"> 数据库名称. </param>
        /// <param name="userName"> 数据库用户名. </param>
        /// <param name="loginName"> login 的名称. </param>
        public static void CreateUser(String connectionString, String databaseName, String userName, String loginName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(databaseName, nameof(databaseName));
            Args.NotEmpty(userName, nameof(userName));

            if (SqlUtil.IsLoginExists(connectionString, loginName) == false)
            {
                throw new InvalidOperationException($"The login [{loginName}] does not exist.");
            }
            if (SqlUtil.IsUserExists(connectionString, databaseName, userName))
            {
                throw new InvalidOperationException($"Cannot create user [{userName}] because it exists.");
            }

            var sql = $"CREATE USER [{userName}] FOR LOGIN [{loginName}] WITH DEFAULT_SCHEMA=[dbo]";
            SqlUtil.ExecuteNonQuery(connectionString, sql);
        }

        /// <summary> 为数据库用户授予 <c>db_owner</c> 角色. </summary>
        /// <param name="connectionString"> 连接字符串.</param>
        /// <param name="userName"> 数据库用户名. </param>
        public static void GrantDbOwner(String connectionString, String userName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(userName, nameof(userName));

            var sql = $"ALTER ROLE [db_owner] ADD MEMBER [{userName}]";
            SqlUtil.ExecuteNonQuery(connectionString, sql);
        }

        public static bool IsTableExists(String connectionString, String schemaName, String tableName)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(schemaName, nameof(schemaName));
            Args.NotEmpty(tableName, nameof(tableName));

            var sql =
                $@"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = '{schemaName}' AND TABLE_NAME = '{tableName}'";
            var count = SqlUtil.ExecuteScalar<int>(connectionString, sql);
            return count > 0;
        }

        public static T ExecuteScalar<T>(String connectionString, String sql, CommandType commandType = CommandType.Text)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(sql, nameof(sql));

            using (var connection = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(sql))
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandType = commandType;
                    Object result = cmd.ExecuteScalar();
                    return (T) result;
                }
            }
        }

        public static void ExecuteNonQuery(String connectionString, String sql, CommandType commandType = CommandType.Text)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(sql, nameof(sql));

            using (var connection = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(sql))
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandType = commandType;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary> 执行 SQL 脚本. </summary>
        /// <param name="connectionString"> 连接字符串. </param>
        /// <param name="sqlScript"> 数据库脚本. 其中可以包含 <c>go</c> 语句. </param>
        /// <remarks> 本方法使用 <see cref="ServerConnection"/> 执行数据库脚本，以便允许在脚本中包含 <c>go</c> 语句等. </remarks>
        public static void ExecuteSqlScript(String connectionString, String sqlScript)
        {
            Args.NotEmpty(connectionString, nameof(connectionString));
            Args.NotEmpty(sqlScript, nameof(sqlScript));

            SqlConnection sqlConnection = new SqlConnection(connectionString);
            ServerConnection svrConnection = new ServerConnection(sqlConnection);
            Server server = new Server(svrConnection);
            server.ConnectionContext.ExecuteNonQuery(sqlScript, ExecutionTypes.Default);
        }
    }
}
