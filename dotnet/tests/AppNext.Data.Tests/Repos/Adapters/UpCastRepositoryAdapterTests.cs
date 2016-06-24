using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos.Core;
using AppBoot.TestCommon;
using Moq;
using NUnit.Framework;

namespace AppBoot.Repos.Adapters
{
    public class UpCastRepositoryAdapterTests
    {
        private Mock<IDerivedSyncRepository> m_AdaptedMock;
        private UpCastRepositoryAdapter<Base, int, Derived, IDerivedSyncRepository> m_Adapter;
        private Base m_Entity;

        [SetUp]
        public void SetUp()
        {
            m_AdaptedMock = new Mock<IDerivedSyncRepository>();
            m_Adapter = Adapt.Up<Base, int, Derived>.From(m_AdaptedMock.Object);
            m_Entity = new Derived();
        }

        /// <summary>
        /// <see cref="UpCastRepositoryAdapter{TDecl,TKey,TImpl,TImplRepository}"/>
        /// throws <see cref="ArgumentNullException"/>
        /// when argument is <c>null</c>.
        /// </summary>
        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(
                () => new UpCastRepositoryAdapter<Base, int, Derived, IDerivedSyncRepository>(null))
                .ForArg("adaptedRepository");
        }

        /// <summary>
        /// <see cref="UpCastRepositoryAdapter{TDecl,TKey,TImpl,TImplRepository}"/>
        /// sets <see cref="UpCastRepositoryAdapter{TDecl,TKey,TImpl,TImplRepository}.AdaptedRepository"/>.
        /// </summary>
        [Test]
        public void Constructor_sets_adaptedRepository()
        {
            var adaptedMock = new Mock<IDerivedSyncRepository>();
            var adapter = Adapt.Up<Base, int, Derived>.From(adaptedMock.Object);
            Assert.NotNull(adapter);
            Assert.AreSame(adaptedMock.Object, adapter.AdaptedRepository);
            Assert.AreSame(adaptedMock.Object, adapter.AdaptedRepository);
        }

        [Test]
        public void RepositoryInfo_is_delegated_to_adaptedRepository()
        {
            var info = new RepositoryInfo(typeof (Derived), typeof (int));
            m_AdaptedMock.CheckCall(() => m_Adapter.Info, m => m.Info, info);
        }

        [Test]
        public void HasChanges_is_delegated_to_adaptedRepository()
        {
            const bool expectedResult = true;
            var hasChangesResult = m_AdaptedMock.CheckCall(
                () => m_Adapter.HasChanges,
                adapted => adapted.HasChanges,
                expectedResult);
            Assert.AreEqual(expectedResult, hasChangesResult);
        }

        [Test]
        public void Insert_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                () => m_Adapter.Insert(m_Entity),
                adapted => adapted.Insert(It.IsAny<Derived>()));
        }

        [Test]
        public void InsertAsnc_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                async () => await m_Adapter.InsertAsync(m_Entity),
                adapted => adapted.InsertAsync(It.IsAny<Derived>()));
        }

        [Test]
        public void Delete_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                () => m_Adapter.Delete(m_Entity),
                adapted => adapted.Delete(It.IsAny<Derived>()));
        }

        [Test]
        public void DeleteAsync_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                async () => await m_Adapter.DeleteAsync(m_Entity),
                adapted => adapted.DeleteAsync(It.IsAny<Derived>()));
        }

        [Test]
        public void Update_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                () => m_Adapter.Update(m_Entity),
                adapted => adapted.Update(It.IsAny<Derived>()));
        }

        [Test]
        public void UpdateAsync_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                async () => await m_Adapter.UpdateAsync(m_Entity),
                adapted => adapted.UpdateAsync(It.IsAny<Derived>()));
        }
        
        [Test]
        public void Reload_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                () => m_Adapter.Reload(m_Entity),
                adapted => adapted.Reload(It.IsAny<Derived>()));
        }

        [Test]
        public void ReloadAsync_is_delegated_to_adaptedRepository()
        {
            m_AdaptedMock.CheckCall(
                async () => await m_Adapter.ReloadAsync(m_Entity),
                adapted => adapted.ReloadAsync(It.IsAny<Derived>()));
        }

        [Test]
        public void Find_is_delegated_to_adaptedRepository()
        {
            const int key = 1;
            var findResult = m_AdaptedMock.CheckCall(
                () => m_Adapter.Find(key),
                adapted => adapted.Find(It.Is<int>(p => p == key)), m_Entity);
            Assert.AreSame(m_Entity, findResult);
        }

        [Test]
        public void FindAsync_is_delegated_to_adaptedRepository()
        {
            const int key = 1;
            var findResult = m_AdaptedMock.CheckCall(
                () => (Derived)m_Adapter.FindAsync(key).Result,
                adapted => adapted.FindAsync(It.Is<int>(p => p == key)), (Derived)m_Entity);
            Assert.AreSame(m_Entity, findResult);
        }

        [Test]
        public void Query_is_delegated_to_adaptedRepository()
        {
            IList<Derived> list = new List<Derived>
            {
                new Derived {Id = 1},
                new Derived {Id = 2}
            };

            Func<IQueryable<Base>, IQueryable<int>> queryBuilder =
                q => q.Select(item => item.Id);
            var expectedResult = queryBuilder(list.AsQueryable()).ToList();

            var queryResult = m_AdaptedMock.CheckCall(
                () => m_Adapter.Fetch(queryBuilder),
                adapted => adapted.Fetch<int>(It.IsAny<Func<IQueryable<Base>, IQueryable<int>>>()),
                expectedResult);
            Assert.AreSame(expectedResult, queryResult);
        }

        [Test]
        public void QueryAsync_is_delegated_to_adaptedRepository()
        {
            IList<Derived> list = new List<Derived>
            {
                new Derived {Id = 1},
                new Derived {Id = 2}
            };

            Func<IQueryable<Base>, IQueryable<int>> queryBuilder =
                q => q.Select(item => item.Id);
            var expectedResult = queryBuilder(list.AsQueryable()).ToList();

            var queryResult = m_AdaptedMock.CheckCall(
                () => m_Adapter.FetchAsync(queryBuilder).Result,
                adapted => adapted.FetchAsync<int>(It.IsAny<Func<IQueryable<Base>, IQueryable<int>>>()),
                expectedResult);
            Assert.AreSame(expectedResult, queryResult);
        }
    }
}
