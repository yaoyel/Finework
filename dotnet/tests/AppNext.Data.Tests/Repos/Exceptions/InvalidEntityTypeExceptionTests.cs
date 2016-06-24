using System;
using AppBoot.Common;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    [TestFixture]
    public class InvalidEntityTypeExceptionTests
    {
        [Test]
        public void ExceptionType_matches_the_guidelines()
        {
            ExceptionTestUtil.CheckGuidelines<InvalidEntityTypeException>();
        }

        [Test]
        public void Constructor_creates_exception_message()
        {
            var expectedType = typeof (String);
            var actualType = typeof (Int32);

            var exception = new InvalidEntityTypeException(expectedType, actualType);
            var expectedMessage = String.Format(InvalidEntityTypeException.m_ErrorFmt, actualType, expectedType);
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new InvalidEntityTypeException(null, typeof (String)))
                .ForArg("expectedType");
            Assert.Throws<ArgumentNullException>(() => new InvalidEntityTypeException(typeof(String), null))
                .ForArg("actualType");
        }
    }
}