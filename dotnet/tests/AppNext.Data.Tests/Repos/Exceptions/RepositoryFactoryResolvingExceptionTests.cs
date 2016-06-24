using System;
using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    [TestFixture]
    public class RepositoryFactoryResolvingExceptionTests
    {
        [Test]
        public void ExceptionType_matches_the_guidelines()
        {
            ExceptionTestUtil.CheckGuidelines<RepositoryFactoryResolvingException>();
        }

        /// <summary>
        /// Checks the <see cref="RepositoryFactoryResolvingException(Type)"/>
        /// builds the exception message.
        /// </summary>
        [Test]
        public void Constructor_creates_exception_message()
        {
            var t = typeof (String);
            var exception = new RepositoryFactoryResolvingException(t);
            var expectedMessage = String.Format(RepositoryFactoryResolvingException.m_ErrorFmt, t);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new RepositoryFactoryResolvingException((Type) null));
        }
    }
}