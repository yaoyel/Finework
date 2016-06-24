using System.Data.Entity;
using AppBoot.Repos.Aef.DbRequired.Shop.NoCast;
using AppBoot.Repos.Aef.DbRequired.Shop.UpCast;
using AppBoot.Repos.Aef.DbRequired.Simple;
using NUnit.Framework;

namespace AppBoot.Repos.Aef.DbRequired
{
    [SetUpFixture]
    public class DbSetup
    {
        [SetUp]
        public void SetUp()
        {
            //Only the first context should drop the test database
            AefTestUtil.InitializeContext(new DropCreateDatabaseAlways<SimpleContext>(), true);

            //Other contexts should NOT drop the test database again
            AefTestUtil.InitializeContext(new CreateDatabaseIfNotExists<NoCastContext>(), true);
            AefTestUtil.InitializeContext(new CreateDatabaseIfNotExists<UpCastContext>(), true);
        }


        [TearDown]
        public void TearDown()
        {
        }
    }
}