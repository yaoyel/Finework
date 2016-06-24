using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppBoot.Repos
{
    /// <summary>
    /// Checks the expections after each stage for <b>async</b> <see cref="IRepository{T, TKey}"/> operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class AsyncStageChecker<T, TKey>
        where T : class
    {
        public AsyncStageChecker(
            IRepository<T, TKey> repository,
            Func<TKey, T> createHandler,
            Action<T> editHandler,
            Func<Task> saveChangesHandler,
            StageExpects expects)
        {
            this.Repository = repository;
            this.CreateHandler = createHandler;
            this.EditHandler = editHandler;
            this.SaveChangesHandler = saveChangesHandler;
            this.Expects = expects;
        }

        private IRepository<T, TKey> Repository { get; set; }

        private Func<TKey, T> CreateHandler { get; set; }

        private Action<T> EditHandler { get; set; }

        private Func<Task> SaveChangesHandler { get; set; }

        private StageExpects Expects { get; set; }

        public async Task ExecuteAsync(TKey key)
        {
            if (CreateHandler == null) throw new InvalidOperationException("OnCreate not set.");
            if (EditHandler == null) throw new InvalidOperationException("OnEdit not set.");
            if (Expects == null) throw new InvalidOperationException("Expects not set.");

            await CheckStageAsync(key,
                "BeforeCreate",
                expectedHasChanges: false,
                expectedFound: false);

            var c = CreateHandler(key);
            await CheckStageAsync(key,
                "AfterCreate",
                expectedHasChanges: false,
                expectedFound: false);

            await SaveChangesAsync();
            await CheckStageAsync(key,
                "AfterCreateSaveChanges",
                expectedHasChanges: false,
                expectedFound: false);

            await Repository.InsertAsync(c);
            await CheckStageAsync(key,
                "AfterInsert",
                expectedHasChanges: Expects.AfterInsert_HasChanges,
                expectedFound: Expects.AfterInsert_Found);

            await SaveChangesAsync();
            await CheckStageAsync(key,
                "AfterInsertSaveChanges",
                expectedHasChanges: false,
                expectedFound: true);

            EditHandler(c);
            await CheckStageAsync(key,
                "AfterEdit",
                expectedHasChanges: Expects.AfterEdit_HasChanges,
                expectedFound: true);

            await Repository.UpdateAsync(c);
            await CheckStageAsync(key,
                "AfterUpdate",
                expectedHasChanges: Expects.AfterUpdate_HasChanges,
                expectedFound: true);

            await SaveChangesAsync();
            await CheckStageAsync(key,
                "AfterUpdateSaveChanges",
                expectedHasChanges: false,
                expectedFound: true);

            await Repository.DeleteAsync(c);
            await CheckStageAsync(key,
                "AfterDelete",
                expectedHasChanges: Expects.AfterDelete_HasChanges,
                expectedFound: Expects.AfterDelete_Found);

            await SaveChangesAsync();
            await CheckStageAsync(key, 
                "AfterDeleteSaveChanges",
                expectedHasChanges: false,
                expectedFound: false);
        }

        private async Task SaveChangesAsync()
        {
            if (SaveChangesHandler != null)
            {
                await SaveChangesHandler();
            }
        }

        private async Task CheckStageAsync(TKey key, String stage, bool expectedHasChanges, bool expectedFound)
        {
            await CheckStageAsync(this.Repository, key, stage, expectedHasChanges, expectedFound);
        }

        private static async Task CheckStageAsync(IRepository<T, TKey> repository, TKey key,
            String stage, bool expectedHasChanges, bool expectedFound)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (String.IsNullOrEmpty(stage)) throw new ArgumentException("stage is null or empty.", "stage");

            Assert.AreEqual(expectedHasChanges, repository.HasChanges, "Stage [{0}] - HasChanges", stage);
            var obj = await repository.FindAsync(key);
            Assert.AreEqual(expectedFound, obj != null, "Stage [{0}] - IsFound", stage);
        }
    }
}