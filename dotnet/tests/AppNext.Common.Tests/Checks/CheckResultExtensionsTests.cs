using System;
using NUnit.Framework;

namespace AppBoot.Checks
{
    [TestFixture]
    public class CheckResultExtensionsTests
    {
        [Test]
        public void ThrowIfNotSucceed_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CheckResultExtensions.ThrowIfFailed<ICheckResult>(null));
        }

        [Test]
        public void ThrowIfNotSucceed_does_NOT_throw_if_IsSucceed_is_true()
        {
            var cr = new CheckResult(true, null);
            cr.ThrowIfFailed();
        }

        [Test]
        public void ThrowIfNotSucceed_throws_if_IsSucceed_is_false()
        {
            var cr = new CheckResult(false, null);
            Assert.Throws<ApplicationException>(() => cr.ThrowIfFailed());
        }
    }
}