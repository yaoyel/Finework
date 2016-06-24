using NUnit.Framework;

namespace AppBoot.Repos.Exceptions
{
    [TestFixture]
    public class RepositoryExceptionTests
    {
        [Test]
        public void ExceptionType_matches_the_guidelines()
        {
            ExceptionTestUtil.CheckGuidelines<RepositoryException>();
        }
    }
}
