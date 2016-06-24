using Moq;
using NUnit.Framework;

namespace AppBoot.Repos.Adapters
{
    [TestFixture]
    public class AdaptTests
    {
        [Test]
        public void Up()
        {
            var adaptedMock = new Mock<IDerivedSyncRepository>();
            var adapter = Adapt.Up<Base, int, Derived>.From(adaptedMock.Object);
            Assert.NotNull(adapter);
            Assert.AreSame(adaptedMock.Object, adapter.AdaptedRepository);
        }
    }
}
