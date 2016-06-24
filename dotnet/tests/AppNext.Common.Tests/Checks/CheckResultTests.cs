using System;
using NUnit.Framework;

namespace AppBoot.Checks
{
    /// <summary> Unit tests for <see cref="CheckResult"/>. </summary>
    [TestFixture]
    public class CheckResultTests
    {
        [Test]
        public void Constructor_sets_property_values(
            [Values(true, false)] bool expectedSucceed, 
            [Values(null, "", "AMessage")] String expectedMessage)
        {
            var cr = new CheckResult(expectedSucceed, expectedMessage);
            Assert.AreEqual(expectedSucceed, cr.IsSucceed);
            Assert.AreEqual(expectedMessage, cr.Message);
        }

        [Test]
        public void CreateException_returns_exception_with_message(
            [Values("", "AMessage")] String expectedMessage)
        {
            var cr = new CheckResult(false, expectedMessage);
            var ex = cr.CreateException(cr.Message);
            Assert.AreEqual(expectedMessage, ex.Message);
        }
    }
}
