using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppBoot.Repos.Aef.DbRequired.Simple
{
    /// <summary>
    /// Unit tests for <see cref="AefRepository{T,TKey}"/>
    /// which require the database connection.
    /// </summary>
    [TestFixture]
    public class AefRepositoryDbTests : TxFixtureBase
    {
        private SimpleContext DbContext { get; set; }

        public override void SetUp()
        {
            base.SetUp();
            this.DbContext = new SimpleContext(true);
        }

        public override void TearDown()
        {
            this.DbContext.Dispose();
            base.TearDown();
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Insert"/>
        /// inserts the entity into a <see cref="DbContext"/>.
        /// </summary>
        [Test]
        public void Insert_inserts_entity_into_DbContext()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            repository.Insert(obj);
            Assert.AreEqual(EntityState.Unchanged, this.DbContext.Entry(obj).State);
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.InsertAsync"/>
        /// inserts the entity into a <see cref="DbContext"/>.
        /// </summary>
        [Test]
        public async Task InsertAsync_inserts_entity_into_DbContext()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            await repository.InsertAsync(obj);
            Assert.AreEqual(EntityState.Unchanged, this.DbContext.Entry(obj).State);
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Delete"/>
        /// deletes the entity when it's state is <see cref="EntityState.Unchanged"/>.
        /// </summary>
        [Test]
        public void Delete_deletes_entity_when_it_is_Unchanged()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            repository.Insert(obj);
            Assert.AreEqual(EntityState.Unchanged, this.DbContext.Entry(obj).State);

            repository.Delete(obj);
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            Assert.IsNull(repository.Find(1));
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.DeleteAsync"/>
        /// deletes the entity when it's state is <see cref="EntityState.Unchanged"/>.
        /// </summary>
        [Test]
        public async Task DeleteAsync_deletes_entity_when_it_is_Unchanged()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            await repository.InsertAsync(obj);
            Assert.AreEqual(EntityState.Unchanged, this.DbContext.Entry(obj).State);

            await repository.DeleteAsync(obj);
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            Assert.IsNull(await repository.FindAsync(1));
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Delete"/>
        /// deletes the entity when it's state is <see cref="EntityState.Modified"/>.
        /// </summary>
        [Test]
        public void Delete_deletes_entity_when_it_is_Modified()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            repository.Insert(obj);
            obj.Name = "Changed";
            Assert.AreEqual(EntityState.Modified, this.DbContext.Entry(obj).State);

            repository.Delete(obj);
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            Assert.IsNull(repository.Find(1));
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.DeleteAsync"/>
        /// deletes the entity when it's state is <see cref="EntityState.Modified"/>.
        /// </summary>
        [Test]
        public async Task DeleteAsync_deletes_entity_when_it_is_Modified()
        {
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);

            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            await repository.InsertAsync(obj);
            obj.Name = "Changed";
            Assert.AreEqual(EntityState.Modified, this.DbContext.Entry(obj).State);

            await repository.DeleteAsync(obj);
            Assert.AreEqual(EntityState.Detached, this.DbContext.Entry(obj).State);
            Assert.IsNull(await repository.FindAsync(1));
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Delete"/>
        /// throws <see cref="InvalidOperationException"/>
        /// when the entity's state is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public void Delete_deletes_entity_when_entity_is_Detached()
        {
            var obj = new SimpleEntity { Id = 1, Name = "Simple" };

            //Inserts an entity
            using (var firstContext = this.DbContext)
            {
                var repository = new AefRepository<SimpleEntity, int>(firstContext);

                repository.Insert(obj);
                Assert.AreEqual(EntityState.Unchanged, firstContext.Entry(obj).State);
            }

            //Deletes an entity which is in the Detached state will cause error
            using (var secondContext = new SimpleContext(true))
            {
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var secondRepository = new AefRepository<SimpleEntity, int>(secondContext);
                secondRepository.Delete(obj);
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.DeleteAsync"/>
        /// throws <see cref="InvalidOperationException"/>
        /// when the entity's state is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public async Task DeleteAsync_deletes_entity_when_entity_is_Detached()
        {
            var obj = new SimpleEntity { Id = 1, Name = "Simple" };

            //Inserts an entity
            using (var firstContext = this.DbContext)
            {
                var repository = new AefRepository<SimpleEntity, int>(firstContext);

                await repository.InsertAsync(obj);
                Assert.AreEqual(EntityState.Unchanged, firstContext.Entry(obj).State);
            }

            //Deletes an entity which is in the Detached state will cause error
            using (var secondContext = new SimpleContext(true))
            {
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var secondRepository = new AefRepository<SimpleEntity, int>(secondContext);
                await secondRepository.DeleteAsync(obj);
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Update"/>
        /// attaches an entity and updates it to the underlying database
        /// when the entity is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public void Update_updates_entity_when_it_is_Detached()
        {
            //inserts an entity
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                repository.Insert(obj);
            }

            //updates a detached entity
            using (var secondContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Changed" };
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var repository = new AefRepository<SimpleEntity, int>(secondContext);
                repository.Update(obj);
                Assert.AreEqual(EntityState.Unchanged, secondContext.Entry(obj).State);
            }

            //verifies the entity has been updated
            using (var thirdContext = new SimpleContext(true))
            {
                var repository = new AefRepository<SimpleEntity, int>(thirdContext);
                var loaded = repository.Find(1);
                Assert.AreEqual("Changed", loaded.Name);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.UpdateAsync"/>
        /// attaches an entity and updates it to the underlying database
        /// when the entity is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public async Task UpdateAsync_updates_entity_when_it_is_Detached()
        {
            //inserts an entity
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                await repository.InsertAsync(obj);
            }

            //updates a detached entity
            using (var secondContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Changed" };
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var repository = new AefRepository<SimpleEntity, int>(secondContext);
                await repository.UpdateAsync(obj);
                Assert.AreEqual(EntityState.Unchanged, secondContext.Entry(obj).State);
            }

            //verifies the entity has been updated
            using (var thirdContext = new SimpleContext(true))
            {
                var repository = new AefRepository<SimpleEntity, int>(thirdContext);
                var loaded = await repository.FindAsync(1);
                Assert.AreEqual("Changed", loaded.Name);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Reload"/>
        /// attaches and reloads the entity 
        /// when it is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public void Reload_reloads_when_entity_is_detached()
        {
            //inserts an entity
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                repository.Insert(obj);
            }

            //reloads an detached entity
            using (var secondContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity {Id = 1};
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var repository = new AefRepository<SimpleEntity, int>(secondContext);
                repository.Reload(obj);
                Assert.AreEqual("Simple", obj.Name);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.ReloadAsync"/>
        /// attaches and reloads the entity 
        /// when it is <see cref="EntityState.Detached"/>.
        /// </summary>
        [Test]
        public async Task ReloadAsync_reloads_when_entity_is_detached()
        {
            //inserts an entity
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                await repository.InsertAsync(obj);
            }

            //reloads an detached entity
            using (var secondContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1 };
                Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj).State);

                var repository = new AefRepository<SimpleEntity, int>(secondContext);
                await repository.ReloadAsync(obj);
                Assert.AreEqual("Simple", obj.Name);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Reload"/>
        /// reloads the entity when it is attached.
        /// </summary>
        [Test]
        public void Reload_reloads_when_entity_is_attached()
        {
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };

                //inserts an entity using the first context
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                repository.Insert(obj);

                //updates the entity using a second context
                using (var secondContext = new SimpleContext(true))
                {
                    var obj2 = new SimpleEntity { Id = 1, Name = "Changed" };
                    Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj2).State);

                    var updateRepository = new AefRepository<SimpleEntity, int>(secondContext);
                    updateRepository.Update(obj2);
                }

                obj.Name = "ChangedThatWillBeDiscarded";

                //reloads the entity using the first context
                repository.Reload(obj);
                Assert.AreEqual("Changed", obj.Name);
            }
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.ReloadAsync"/>
        /// reloads the entity when it is attached.
        /// </summary>
        [Test]
        public async Task ReloadAsync_reloads_when_entity_is_attached()
        {
            using (var firstContext = new SimpleContext(true))
            {
                var obj = new SimpleEntity { Id = 1, Name = "Simple" };

                //inserts an entity using the first context
                var repository = new AefRepository<SimpleEntity, int>(firstContext);
                await repository.InsertAsync(obj);

                //updates the entity using a second context
                using (var secondContext = new SimpleContext(true))
                {
                    var obj2 = new SimpleEntity { Id = 1, Name = "Changed" };
                    Assert.AreEqual(EntityState.Detached, secondContext.Entry(obj2).State);

                    var updateRepository = new AefRepository<SimpleEntity, int>(secondContext);
                    await updateRepository.UpdateAsync(obj2);
                }

                obj.Name = "ChangedThatWillBeDiscarded";

                //reloads the entity using the first context
                await repository.ReloadAsync(obj);
                Assert.AreEqual("Changed", obj.Name);
            }
        }
        
        /// <summary>
        /// <see cref="AefRepository{T,TKey}.Fetch{TResult}"/>
        /// queries the database.
        /// </summary>
        [Test]
        public void Query_queries_database()
        {
            var obj = new SimpleEntity {Id = 1, Name = "Simple"};
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);
            repository.Insert(obj);

            var list = repository.Fetch(q => q);
            Assert.NotNull(list);
            Assert.AreEqual(obj.Id, list.Single().Id);
        }

        /// <summary>
        /// <see cref="AefRepository{T,TKey}.FetchAsync{TResult}"/>
        /// queries the database.
        /// </summary>
        [Test]
        public async Task QueryAsync_queries_database()
        {
            var obj = new SimpleEntity { Id = 1, Name = "Simple" };
            var repository = new AefRepository<SimpleEntity, int>(this.DbContext);
            await repository.InsertAsync(obj);

            var list = await repository.FetchAsync(q => q);
            Assert.NotNull(list);
            Assert.AreEqual(obj.Id, list.Single().Id);
        }
    }
}