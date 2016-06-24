using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace AppBoot.Repos
{
    [TestFixture]
    public class RepositoryExtensionsTests
    {
        /// <summary>
        /// The expression for the method call of <see cref="IRepository{T}.Fetch{TResult}(Func{IQueryable{T}, IQueryable{TResult}})"/> 
        /// </summary>
        private static readonly Expression<Func<IRepository<SimpleEntity>, IList<SimpleEntity>>> m_FetchCall
            = r => r.Fetch<SimpleEntity>(It.IsAny<Func<IQueryable<SimpleEntity>, IQueryable<SimpleEntity>>>());

        /// <summary>
        /// The expression for the method call of <see cref="IRepository{T}.FetchAsync{TResult}(Func{IQueryable{T}, IQueryable{TResult}})"/> 
        /// </summary>
        private static readonly Expression<Func<IRepository<SimpleEntity>, Task<IList<SimpleEntity>>>> m_FetchAsyncCall
            = r => r.FetchAsync<SimpleEntity>(It.IsAny<Func<IQueryable<SimpleEntity>, IQueryable<SimpleEntity>>>());

        [Test]
        public void Fetch_for_expression_calls_Fetch_for_IQueryable()
        {
            var rm = new Mock<IRepository<SimpleEntity>>();
            rm.Setup(m_FetchCall).Verifiable();

            RepositoryExtensions.Fetch(rm.Object, entity => entity.Name != null);
            rm.Verify(m_FetchCall, Times.Once);
        }

        [Test]
        public void FetchAsync_for_expression_calls_FetchAsync_for_IQueryable()
        {
            var innerTask = new Task<IList<SimpleEntity>>(() => new List<SimpleEntity>());
            
            var rm = new Mock<IRepository<SimpleEntity>>();
            rm.Setup(m_FetchAsyncCall).Returns(innerTask).Verifiable();

            var resultTask = RepositoryExtensions.FetchAsync(rm.Object, entity => entity.Name != null);

            rm.Verify(m_FetchAsyncCall, Times.Once);
            Assert.AreSame(innerTask, resultTask);
        }

        [Test]
        public void FetchAll_calls_Fetch_for_IQueryable()
        {
            var rm = new Mock<IRepository<SimpleEntity>>();
            rm.Setup(m_FetchCall).Verifiable();

            RepositoryExtensions.FetchAll(rm.Object);
            rm.Verify(m_FetchCall);
        }

        [Test]
        public void FetchAllAsync_calls_FetchAsync_for_IQueryable()
        {
            var rm = new Mock<IRepository<SimpleEntity>>();
            rm.Setup(m_FetchAsyncCall).Verifiable();

            RepositoryExtensions.FetchAllAsync(rm.Object);
            rm.Verify(m_FetchAsyncCall);
        }
    }
}
