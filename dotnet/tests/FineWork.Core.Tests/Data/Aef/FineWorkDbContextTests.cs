using System.Data.Entity;
using System.Linq;
using AppBoot.TestCommon;
using FineWork.Logging;
using NUnit.Framework;

namespace FineWork.Data.Aef
{
    [TestFixture]
    public class FineWorkDbContextTest
    {
        [Test]
        public void FineWork_database_is_connectable()
        {
            var cs = SqlTestUtil.GetConnectionStringFromName("FineWork");
            SqlTestUtil.CheckConnectionStringIsConnectable(cs);
        }

        [Test]
        public void FineWorkDbContext_is_connectable()
        {
            var context = new FineWorkDbContext(LogManager.Factory);
            int result = context.Database.SqlQuery<int>("SELECT 1").Single();
            Assert.AreEqual(1, result);
        }

        [Ignore("This test drops and recreates the database. It requires the 'disableDatabaseInitialization' set to 'true' in entityFramework.config .")]
        [Test]
        public void FineWorkDbContext_Initialize()
        {
            Database.SetInitializer(new FineWorkDbContextInitializer());
            var context = new FineWorkDbContext(LogManager.Factory);
            context.Database.Initialize(true);
            int result = context.Database.SqlQuery<int>("SELECT 1").Single();
            Assert.AreEqual(1, result);
        }
    }
}
