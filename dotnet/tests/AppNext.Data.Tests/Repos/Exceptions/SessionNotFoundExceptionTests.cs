using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    [TestFixture]
    public class SessionNotFoundExceptionTests
    {
        [Test]
        public void ExceptionType_matches_the_guidelines()
        {
            ExceptionTestUtil.CheckGuidelines<SessionNotFoundException>();
        }
    }
}