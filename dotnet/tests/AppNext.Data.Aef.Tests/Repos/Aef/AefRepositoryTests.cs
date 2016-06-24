using System;
using AppBoot.Repos.Exceptions;
using AppBoot.TestCommon;
using Moq;
using NUnit.Framework;

namespace AppBoot.Repos.Aef
{
    [TestFixture]
    public class AefRepositoryTests
    {
        private Mock<AefSession> m_SessionMock;
        
        [SetUp]
        public void SetUp()
        {
            m_SessionMock = new Mock<AefSession>();
        }

        [TearDown]
        public void TearDown()
        {
        }

        private AefRepository<SimpleEntity, int> CreateRepository()
        {
            return new AefRepository<SimpleEntity, int>(m_SessionMock.Object);
        }

        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(
                () => new AefRepository<String, int>((AefSession) null))
                .ForArg("session");

            Assert.Throws<ArgumentNullException>(
                () => new AefRepository<String, int>((ISessionProvider<AefSession>)null))
                .ForArg("sessionProvider");
        }

        [Test]
        public void Info_returns_generic_argument_types()
        {
            var repository = CreateRepository();
            var info = repository.Info;
            Assert.AreEqual(typeof(SimpleEntity), info.EntityType);
            Assert.AreEqual(typeof(int), info.KeyType);
        }
        
        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Insert"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void Insert_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repository.Insert(null)).ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.InsertAsync"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void InsertAsync_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(
                async () => await repository.InsertAsync(null))
                .ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Delete"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void Delete_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repository.Delete(null)).ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.DeleteAsync"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void DeleteAsync_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(
                async () => await repository.DeleteAsync(null))
                .ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Update"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void Update_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repository.Update(null)).ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.UpdateAsync"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void UpdateAsync_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(
                async () => await repository.UpdateAsync(null))
                .ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Reload"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void Reload_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repository.Reload(null)).ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.ReloadAsync"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void ReloadAsync_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(
                async () => await repository.ReloadAsync(null))
                .ForArg("entity");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Fetch{TResult}"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void Query_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(() => repository.Fetch<Object>(null)).ForArg("queryBuilder");
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.FetchAsync{TResult}"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is null.
        /// </summary>
        [Test]
        public void QueryAsync_throws_when_argument_is_null()
        {
            var repository = CreateRepository();
            Assert.Throws<ArgumentNullException>(
                async () => await repository.FetchAsync<Object>(null))
                .ForArg("queryBuilder");
        }
    }
}