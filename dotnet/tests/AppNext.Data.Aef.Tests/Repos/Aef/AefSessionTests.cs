using System.Data.Entity;
using NUnit.Framework;

namespace AppBoot.Repos.Aef
{
    [TestFixture]
    public class AefSessionTests
    {
        private class DbContextTestSupport : DbContext
        {
            public DbContextTestSupport()
                : base("AppBoot")
            {
            }

            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }
    }
}
