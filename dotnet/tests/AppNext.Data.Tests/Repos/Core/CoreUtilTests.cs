using System;
using AppBoot.Common;
using AppBoot.Repos.Exceptions;
using AppBoot.TestCommon;
using Moq;
using NUnit.Framework;

namespace AppBoot.Repos.Core
{
    [TestFixture]
    public class CoreUtilTests
    {
        [Test]
        public void CheckedArg_throws_when_arg_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CoreUtil.CheckedArg<Object>(null, "arg"))
                .ForArg("arg");
        }

        [Test]
        public void CheckedArg_returns_the_arg_when_arg_is_not_null()
        {
            Object arg = new Object();
            var result = CoreUtil.CheckedArg(arg, "arg");
            Assert.AreSame(arg, result);
        }

        [Test]
        public void CastEntity_throws_when_arg_is_null()
        {
            var repositoryMock = new Mock<IRepository<Object>>();

            Assert.Throws<ArgumentNullException>(
                () => CoreUtil.CastEntity<Object, Object>(null, new object()))
                .ForArg("repository");
            Assert.Throws<ArgumentNullException>(
                () => CoreUtil.CastEntity<Object, Object>(repositoryMock.Object, null))
                .ForArg("entity");
        }

        [Test]
        public void CastEntity_returns_casted_entity()
        {
            var repositoryMock = new Mock<IRepository<String>>();
            Object casted = CoreUtil.CastEntity<Object, String>(repositoryMock.Object, "Hello");
            Assert.IsInstanceOf<String>(casted);
        }

        [Test]
        public void CastEntity_throws_when_casting_fails()
        {
            var repositoryMock = new Mock<IRepository<String>>();

            Assert.Throws<InvalidEntityTypeException>(
                () => CoreUtil.CastEntity<Object, String>(repositoryMock.Object, new object()));
        }
    }
}
