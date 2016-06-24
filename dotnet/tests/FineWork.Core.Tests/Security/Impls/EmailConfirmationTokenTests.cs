using System;
using System.Globalization;
using NUnit.Framework;

namespace FineWork.Security.Impls
{
    [TestFixture]
    public class EmailConfirmationTokenTests
    {
        private static readonly DateTime m_GeneratedAt = new DateTime(1998, 1, 2, 3, 4, 5);

        [Test]
        public void Parse_SHOULD_support_ToUriQueryArg()
        {
            var token = new EmailConfirmationToken(m_GeneratedAt);
            var s = token.ToUriQueryArg();

            var parsed = EmailConfirmationToken.Parse(s);
            Assert.AreEqual(m_GeneratedAt, parsed.GeneratedAt);
        }

        [Test]
        public void IsBefore_SHOULD_relative_to_Now()
        {
            var now = DateTime.Now;
            var oneDayEarlier = now.Subtract(TimeSpan.FromDays(1));

            var token = new EmailConfirmationToken(oneDayEarlier);

            Assert.IsTrue(token.IsBefore(TimeSpan.Zero));
            Assert.IsTrue(token.IsBefore(TimeSpan.FromHours(23.99)));
            Assert.IsFalse(token.IsBefore(TimeSpan.FromHours(24.01)));
        }
    }
}
