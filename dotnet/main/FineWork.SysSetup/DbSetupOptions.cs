using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace FineWork.SysSetup
{
    public class DbSetupOptions
    {
        [Option("csname", DefaultValue = "FineWork", HelpText = "The name of the connection string specified in connectionStrings.config")]
        public String ConnectionStringName { get; set; }

        [Option("cs", Required = false,
            HelpText = "The connection string. It will take precedence over the 'csname' option.")]
        public String ConnectionString
        {
            get
            {
                return m_ConnectionString ??
                       (m_ConnectionString = !String.IsNullOrEmpty(ConnectionStringName)
                           ? ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString
                           : null);
            }
            set { m_ConnectionString = value; }
        }

        private String m_ConnectionString;

        private bool m_IsConnectionStringBuilt;

        private String m_TargetConnectionString;

        public String TargetConnectionString
        {
            get
            {
                return m_TargetConnectionString ??
                       (m_TargetConnectionString = SqlUtil.ChangeDatabase(this.ConnectionString, DatabaseName));
            }
        }

        private String m_MasterConnectionString;

        public String MasterConnectionString
        {
            get
            {
                return m_MasterConnectionString ??
                       (m_MasterConnectionString = SqlUtil.ChangeToMaster(this.ConnectionString));
            }
        }


        [Option("dropobj", DefaultValue = false, HelpText = "Drop schema objects (tables, views, etc).")]
        public bool IncludeDropSchemaObjects { get; set; }

        [Option("dropdb", DefaultValue = false, HelpText = "Drop the database.")]
        public bool IncludeDropDatabase { get; set; }

        [Option("createdb", DefaultValue = false, HelpText = "Create a new database.")]
        public bool IncludeCreateDatabase { get; set; }

        [Option("createlogin", DefaultValue = false, HelpText = "Creates a database login specified by 'login'.")]
        public bool IncludeCreateLogin { get; set; }

        [Option("createuser", DefaultValue = false, HelpText = "Creates a database user specified by 'dbuser'.")]
        public bool IncludeCreateUser { get; set; }

        [Option("createobj", DefaultValue = false, HelpText = "Create schema objects (tables, views, etc).")]
        public bool IncludeCreateSchemaObjects { get; set; }

        [Option("seed", DefaultValue = false, HelpText = "Add sample records.")]
        public bool IncludeSeed { get; set; }

        [Option("bulkseed", DefaultValue = false, HelpText = "Add bulk sample records.")]
        public bool IncludeBulkSeed { get; set; }

        [Option("dbdir", HelpText = "The directory where to create the database files.")]
        public String DatabaseDir { get; set; }

        [Option("dbname", DefaultValue = "FineWork", HelpText = "The database name.")]
        public String DatabaseName { get; set; }

        [Option("login", DefaultValue = @"NT AUTHORITY\NETWORK SERVICE", HelpText = "The server login.")]
        public String LoginName { get; set; }

        [Option("dbuser", DefaultValue = @"NT AUTHORITY\NETWORK SERVICE", HelpText = "The database user name.")]
        public String UserName { get; set; }

        public void PrintTo(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteLine("=========================================================");
            writer.WriteLine("Options:");
            writer.WriteLine("---------------------------------------------------------");
            writer.WriteLine($"ConnectionStringName : {this.ConnectionStringName}");
            writer.WriteLine($"ConnectionString: {this.ConnectionString}");
            writer.WriteLine($"MasterConnectionString: {this.MasterConnectionString}");
            writer.WriteLine($"TargetConnectionString: {this.TargetConnectionString}");
            writer.WriteLine($"IncludeDropSchemaObjects: {this.IncludeDropSchemaObjects}");
            writer.WriteLine($"IncludeDropDatabase: {this.IncludeDropDatabase}");
            writer.WriteLine($"IncludeCreateDatabase : {this.IncludeCreateDatabase}");
            writer.WriteLine($"IncludeCreateLogin: {this.IncludeCreateLogin}");
            writer.WriteLine($"IncludeCreateUser: {this.IncludeCreateUser}");
            writer.WriteLine($"IncludeCreateSchemaObjects: {this.IncludeDropSchemaObjects}");
            writer.WriteLine($"IncludeSeed: {this.IncludeSeed}");
            writer.WriteLine($"IncludeBulkSeed: {this.IncludeBulkSeed}");

            writer.WriteLine($"DatabaseDir: {this.DatabaseDir}");
            writer.WriteLine($"DatabaseName: {this.DatabaseName}");
            writer.WriteLine($"LoginName: {this.LoginName}");
            writer.WriteLine($"UserName: {this.UserName}");
            writer.WriteLine("=========================================================");
        }
    }
}