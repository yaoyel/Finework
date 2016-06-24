using System;
using NUnit.Framework;

namespace AppBoot.Repos
{
    /// <summary>
    /// Checks the expections after each stage for <b>sync</b> <see cref="IRepository{T, TKey}"/> operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class SyncStageChecker<T, TKey>
        where T : class
    {
        public SyncStageChecker(
            IRepository<T, TKey> repository,
            Func<TKey, T> createHandler,
            Action<T> editHandler,
            Action saveChangesHandler,
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

        private Action SaveChangesHandler { get; set; }

        private StageExpects Expects { get; set; }

        public void Execute(TKey key)
        {
            if (CreateHandler == null) throw new InvalidOperationException("OnCreate not set.");
            if (EditHandler == null) throw new InvalidOperationException("OnEdit not set.");
            if (Expects == null) throw new InvalidOperationException("Expects not set.");

            CheckStage(key,
                "BeforeCreate",
                expectedHasChanges: false,
                expectedFound: false);

            var c = CreateHandler(key);
            CheckStage(key,
                "AfterCreate",
                expectedHasChanges: false,
                expectedFound: false);

            SaveChanges();
            CheckStage(key,
                "AfterCreateSaveChanges",
                expectedHasChanges: false,
                expectedFound: false);

            Repository.Insert(c);
            CheckStage(key,
                "AfterInsert",
                expectedHasChanges: Expects.AfterInsert_HasChanges,
                expectedFound: Expects.AfterInsert_Found);

            SaveChanges();
            CheckStage(key,
                "AfterInsertSaveChanges",
                expectedHasChanges: false,
                expectedFound: true);

            EditHandler(c);
            CheckStage(key,
                "AfterEdit",
                expectedHasChanges: Expects.AfterEdit_HasChanges,
                expectedFound: true);

            Repository.Update(c);
            CheckStage(key,
                "AfterUpdate",
                expectedHasChanges: Expects.AfterUpdate_HasChanges,
                expectedFound: true);

            SaveChanges();
            CheckStage(key,
                "AfterUpdateSaveChanges",
                expectedHasChanges: false,
                expectedFound: true);

            Repository.Delete(c);
            CheckStage(key,
                "AfterDelete",
                expectedHasChanges: Expects.AfterDelete_HasChanges,
                expectedFound: Expects.AfterDelete_Found);

            SaveChanges();
            CheckStage(key, 
                "AfterDeleteSaveChanges",
                expectedHasChanges: false,
                expectedFound: false);
        }

        private void SaveChanges()
        {
            if (SaveChangesHandler != null)
            {
                SaveChangesHandler();
            }
        }

        private void CheckStage(TKey key, String stage, bool expectedHasChanges, bool expectedFound)
        {
            CheckStage(this.Repository, key, stage, expectedHasChanges, expectedFound);
        }

        private static void CheckStage(IRepository<T, TKey> repository, TKey key,
            String stage, bool expectedHasChanges, bool expectedFound)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (String.IsNullOrEmpty(stage)) throw new ArgumentException("stage is null or empty.", "stage");

            Assert.AreEqual(expectedHasChanges, repository.HasChanges, "Stage [{0}] - HasChanges", stage);
            var obj = repository.Find(key);
            Assert.AreEqual(expectedFound, obj != null, "Stage [{0}] - IsFound", stage);
        }
    }
}