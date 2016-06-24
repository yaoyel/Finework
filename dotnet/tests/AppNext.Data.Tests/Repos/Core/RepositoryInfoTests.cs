using System;
using AppBoot.Common;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Repos.Core
{
    [TestFixture]
    public class RepositoryInfoTests
    {
        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(
                () => new RepositoryInfo(null, typeof (Object))).ForArg("entityType");
            Assert.Throws<ArgumentNullException>(
                () => new RepositoryInfo(typeof(Object), null)).ForArg("keyType");
        }

        [Test]
        public void Constructor_sets_types()
        {
            var info = new RepositoryInfo(typeof (String), typeof (int));
            Assert.AreEqual(typeof(String), info.EntityType);
            Assert.AreEqual(typeof(int), info.KeyType);
        }
    }
}
