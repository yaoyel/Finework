using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    [TestFixture]
    public class RepositoryResolvingExceptionTests
    {
        [Test]
        public void ExceptionType_matches_the_guidelines()
        {
            ExceptionTestUtil.CheckGuidelines<RepositoryResolvingException>();
        }
    }
}