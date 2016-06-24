using System;
using System.Data;
using System.IO;
using AppBoot.Common;
using FineWork.Data.Aef;

namespace FineWork.SysSetup
{
    /// <summary> Setup the database. </summary>
    public class DbSetup
    {
        public void Execute(DbSetupOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (String.IsNullOrEmpty(options.ConnectionString))
                throw new ArgumentException("The connection string not specified.", nameof(options));

            var csTarget = options.TargetConnectionString;
            var csMaster = options.MasterConnectionString;

            var dbExists = SqlUtil.IsDatabaseExists(csMaster, options.DatabaseName);
            var loginExists = SqlUtil.IsLoginExists(csMaster, options.LoginName);
            var userExists = dbExists && SqlUtil.IsUserExists(csTarget, options.DatabaseName, options.UserName);

            if (options.IncludeDropSchemaObjects)
            {
                ExecuteWhen($"Dropping database [{options.DatabaseName}] schema objects", dbExists, () =>
                {
                    var sql = File.ReadAllText(@"DbScripts\DropSchemaObjects.sql");
                    SqlUtil.ExecuteSqlScript(csTarget, sql);
                });
            }

            if (options.IncludeDropDatabase)
            {
                ExecuteWhen($"Dropping database [{options.DatabaseName}]", dbExists, () =>
                {
                    SqlUtil.DropDatabase(csMaster, options.DatabaseName);
                });
            }

            if (options.IncludeCreateLogin)
            {
                ExecuteWhen($"Creating login [{options.LoginName}]", (!loginExists), () =>
                {
                    SqlUtil.CreateLogin(csMaster, options.LoginName);
                });
            }

            if (options.IncludeCreateDatabase)
            {
                Execute($"Creating database [{options.DatabaseName}]", () =>
                {
                    SqlUtil.CreateDatabase(csMaster, options.DatabaseDir, options.DatabaseName);
                });
            }

            if (options.IncludeCreateUser)
            {
                ExecuteWhen($"Creating database user {options.UserName}", (!userExists), () =>
                {
                    SqlUtil.CreateUser(csTarget, options.DatabaseName, options.UserName, options.LoginName);
                });
                ExecuteWhen($"Grant database user {options.UserName} with [db_owner]", (!userExists), () =>
                {
                    SqlUtil.GrantDbOwner(csTarget, options.UserName);
                });
            }

            if (options.IncludeCreateSchemaObjects)
            {
                Execute("Creating database schema objects", () =>
                {
                    var sql = File.ReadAllText(@"DbScripts\CreateSchemaObjects.sql");
                    SqlUtil.ExecuteSqlScript(csTarget, sql);
                });
            }

            if (options.IncludeSeed)
            {
                Execute("Seeding database objects", () =>
                {
                    using (var dbContext = new FineWorkDbContext(options.TargetConnectionString, false))
                    {
                        DbMiniSeed miniSeed = new DbMiniSeed(dbContext);
                        miniSeed.Seed();
                        dbContext.SaveChanges();
                    }
                });
            }

            if (options.IncludeBulkSeed)
            {
                using (var dbContext = new FineWorkDbContext(options.TargetConnectionString, true))
                {
                    var bulkSeed = new DbBulkSeed(dbContext);
                    bulkSeed.Seed();
                    dbContext.SaveChanges();
                }
            }
        }

        private void Execute(String taskName, Action action)
        {
            ExecuteWhen(taskName, true, action);
        }

        private bool ExecuteWhen(String taskName, bool condition, Action action)
        {
            Args.NotEmpty(taskName, nameof(taskName));
            Args.NotNull(action, nameof(action));

            Console.Out.Write(taskName + " ......");
            if (condition)
            {
                action();
                Console.Out.WriteLine("OK");
            }
            else
            {
                Console.Out.WriteLine("Skipped");
            }
            return condition;
        }
    }
}
