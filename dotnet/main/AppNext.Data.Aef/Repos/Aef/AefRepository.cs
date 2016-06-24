using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Core;
using AppBoot.Repos.Exceptions;

namespace AppBoot.Repos.Aef
{
    /// <summary> Represents the base type for both sync/async repositories which based on Entity Framework. </summary>
    /// <typeparam name="T"> The entity type. It should be a reference type to satisfy <see cref="DbSet{TEntity}"/>. </typeparam>
    /// <typeparam name="TKey"> The primary key type. </typeparam>
    public class AefRepository<T, TKey> : IRepository<T, TKey> 
        where T : class
    {
        public AefRepository(AefSession session)
            : this(session.AsProvider())
        {
        }

        public AefRepository(ISessionProvider<AefSession> sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentNullException("sessionProvider");
            this.m_SessionProvider = sessionProvider;
        }

        #region Session/DbSet

        private readonly ISessionProvider<AefSession> m_SessionProvider;

        protected internal virtual AefSession GetSession()
        {
            return m_SessionProvider.GetSession();
        }

        /// <summary> Gets the <see cref="AefSession"/>. </summary>
        /// <exception cref="SessionNotFoundException"> if there is no session instance. </exception>
        public AefSession Session
        {
            get
            {
                var session = GetSession();
                if (session == null) throw new SessionNotFoundException();
                return session;
            }
        }

        protected virtual DbSet<T> Table
        {
            get { return this.Session.DbContext.Set<T>(); }
        }

        #endregion

        #region Repository change handler

        protected virtual void OnRepositoryChanged(RepositoryChanges change, Object data)
        {
            this.Session.HandleRepositoryChanged(this);
        }

        protected virtual Task OnRepositoryChangedAsync(RepositoryChanges change, Object data)
        {
            return this.Session.HandleRepositoryChangedAsync(this);
        }

        #endregion

        private static readonly IRepositoryInfo m_Info = new RepositoryInfo(typeof(T), typeof(TKey));

        public virtual IRepositoryInfo Info
        {
            get { return m_Info; }
        }

        #region HasChanges

        public virtual bool HasChanges
        {
            get { return this.Session.DbContext.ChangeTracker.HasChanges(); }
        }

        #endregion

        #region Insert/InsertAsync

        public virtual void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            this.Table.Add(entity);

            OnRepositoryChanged(RepositoryChanges.Insert, entity);
        }

        public virtual async Task InsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            this.Table.Add(entity);

            await OnRepositoryChangedAsync(RepositoryChanges.Insert, entity);
        }


        #endregion

        #region Delete/DeleteAsync

        public virtual void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            EnsureAttached(entity);
            this.Table.Remove(entity);

            OnRepositoryChanged(RepositoryChanges.Delete, entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            EnsureAttached(entity);
            this.Table.Remove(entity);

            await OnRepositoryChangedAsync(RepositoryChanges.Delete, entity);
        }

        #endregion

        #region Update/UpdateAsync

        public virtual void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);

            //Put the entity into Modified state to ensure EntityFramework issues an UPDATE statement.
            if (entry.State == EntityState.Unchanged)
            {
                entry.State = EntityState.Modified;
            }

            OnRepositoryChanged(RepositoryChanges.Update, entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);

            //Put the entity into Modified state to ensure EntityFramework issues an UPDATE statement.
            if (entry.State == EntityState.Unchanged)
            {
                entry.State = EntityState.Modified;
            }

            await OnRepositoryChangedAsync(RepositoryChanges.Update, entity);
        }

        #endregion

        #region Reload/ReloadAsync

        public virtual void Reload(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);
            entry.Reload();
        }

        public virtual async Task ReloadAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);

            await entry.ReloadAsync();
        }

        #endregion

        #region Fetch/FetchAsync

        public virtual IList<TResult> Fetch<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            if (queryBuilder == null) throw new ArgumentNullException("queryBuilder");

            var query = queryBuilder(this.Table);
            return query.ToList();
        }

        public virtual async Task<IList<TResult>> FetchAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            if (queryBuilder == null) throw new ArgumentNullException("queryBuilder");

            var query = queryBuilder(this.Table);
            return await query.ToListAsync();
        }

        #endregion

        #region Find/FindAsync

        public virtual T Find(TKey id)
        {
            return this.Table.Find(id);
        }

        public virtual async Task<T> FindAsync(TKey id)
        {
            return await this.Table.FindAsync(id);
        }

        #endregion

        /// <summary> Attaches an entity if it's state is <see cref="EntityState.Detached"/>. </summary>
        /// <remarks> Entity Framework throws exception when trying to 
        /// delete or update an entity which its state is <see cref="EntityState.Detached"/>.
        /// </remarks>
        protected DbEntityEntry<T> EnsureAttached(T entity)
        {
            var entry = this.Session.DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                //  <see cref="EfSyncRepository{T, TKey}.Attach"/>
                //  throws <see cref="InvalidOperationException"/>
                //  if another entity with the same key has been already attached.
                this.Table.Attach(entity);
            }
            return entry;
        }
    }
}