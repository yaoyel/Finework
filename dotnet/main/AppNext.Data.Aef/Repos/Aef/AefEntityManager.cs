using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppBoot.Repos.Core;
using AppBoot.Repos.Exceptions;

namespace AppBoot.Repos.Aef
{
    /// <summary>
    /// <see cref="AefEntityManager{T, TKey}"/> is used to be subclassed by entity management classes 
    /// which applying business rules without a seperated repository layer.
    /// </summary>
    /// <typeparam name="T"> The entity type. </typeparam>
    /// <typeparam name="TKey"> The primary key type. </typeparam>
    public abstract class AefEntityManager<T, TKey> : IRepository
        where T : class
    {
        protected AefEntityManager(ISessionProvider<AefSession> sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentNullException(nameof(sessionProvider));
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

        protected virtual void OnRepositoryChanged(RepositoryChanges change, T data)
        {
            try
            {
                this.Session.HandleRepositoryChanged(this);
            }
            catch (OptimisticConcurrencyException)
            {
                ((IObjectContextAdapter)Session.DbContext).ObjectContext.Refresh(RefreshMode.ClientWins, data);
                this.Session.HandleRepositoryChanged(this);
            }
            catch
            {
                this.Table.Remove(data);
                throw;
            }

        }

        protected virtual Task OnRepositoryChangedAsync(RepositoryChanges change, T data)
        {
            return this.Session.HandleRepositoryChangedAsync(this);
        }

        #endregion

        #region Insert/InsertAsync

        protected virtual void InternalInsert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            this.Table.Add(entity);
            OnRepositoryChanged(RepositoryChanges.Insert, entity);
        }

        protected virtual async Task InternalInsertAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            this.Table.Add(entity);

            await OnRepositoryChangedAsync(RepositoryChanges.Insert, entity);
        }


        #endregion

        #region Delete/DeleteAsync

        protected virtual void InternalDelete(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            EnsureAttached(entity);
            this.Table.Remove(entity);

            OnRepositoryChanged(RepositoryChanges.Delete, entity);
        }

        protected virtual async Task InternalDeleteAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            EnsureAttached(entity);
            this.Table.Remove(entity);

            await OnRepositoryChangedAsync(RepositoryChanges.Delete, entity);
        }

        #endregion

        #region Update/UpdateAsync

        protected virtual void InternalUpdate(T entity)
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

        protected virtual async Task InternalUpdateAsync(T entity)
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

        protected virtual void InternalReload(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);
            entry.Reload();
        }

        protected virtual async Task InternalReloadAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            var entry = EnsureAttached(entity);

            await entry.ReloadAsync();
        }

        #endregion

        #region Fetch/FetchAsync

        protected virtual IList<TResult> InternalFetch<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            if (queryBuilder == null) throw new ArgumentNullException("queryBuilder");

            var query = queryBuilder(this.Table);
            return query.ToList();
        }

        protected virtual async Task<IList<TResult>> InternalFetchAsync<TResult>(
            Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            if (queryBuilder == null) throw new ArgumentNullException("queryBuilder");

            var query = queryBuilder(this.Table);
            return await query.ToListAsync();
        }

        protected IList<T> InternalFetch(Expression<Func<T, bool>> predicate)
        {
            return this.InternalFetch<T>(q => q.Where(predicate));
        }

        protected virtual async Task<IList<T>> InternalFetchAsync(Expression<Func<T, bool>> predicate)
        {
            return await this.InternalFetchAsync(q => q.Where(predicate));
        }

        #endregion

        #region FetchAll

        protected IList<T> InternalFetchAll()
        {
            return this.InternalFetch<T>(q => q);
        }

        protected virtual async Task<IList<T>> InternalFetchAllAsync()
        {
            return await this.InternalFetchAsync(q => q);
        }
        
        #endregion

        #region Find/FindAsync

        protected virtual T InternalFind(TKey id)
        {
            return this.Table.Find(id);
        }

        protected virtual async Task<T> InternalFindAsync(TKey id)
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

        public IRepositoryInfo Info
        {
            get {return new RepositoryInfo(typeof(T), typeof(TKey));}
        }
    }
}
