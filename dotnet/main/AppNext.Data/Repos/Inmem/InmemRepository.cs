using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Core;

namespace AppBoot.Repos.Inmem
{
    public class InmemRepository<T, TKey> : IRepository<T, TKey> 
        where T : class
    {
        public InmemRepository(Func<T, TKey> keyGetter)
        {
            if (keyGetter == null) throw new ArgumentNullException("keyGetter");

            m_InternalList = new EntityKeyedCollection<TKey, T>(keyGetter);
            m_KeyAccessor = keyGetter;
        }

        private readonly EntityKeyedCollection<TKey, T> m_InternalList;

        private readonly Func<T, TKey> m_KeyAccessor;

        protected TKey GetKey(T entity)
        {
            return m_KeyAccessor(entity);
        }

        private static readonly IRepositoryInfo m_Info = new RepositoryInfo(typeof(T), typeof(TKey));

        public virtual IRepositoryInfo Info
        {
            get { return m_Info; }
        }

        public bool HasChanges { get; set; }

        #region Insert/InsertAsync

        public virtual void Insert(T entity)
        {
            m_InternalList.Add(entity);
            this.HasChanges = true;
        }

        public virtual Task InsertAsync(T entity)
        {
            this.Insert(entity);
            return Task.FromResult(0);
        }

        #endregion

        #region Delete/DeleteAsync

        public virtual void Delete(T entity)
        {
            m_InternalList.Remove(entity);
            this.HasChanges = true;
        }

        public virtual Task DeleteAsync(T entity)
        {
            this.Delete(entity);
            return Task.FromResult(0);
        }

        #endregion

        #region Update/UpdateAsync

        public virtual void Update(T entity)
        {
            this.HasChanges = true;
        }

        public virtual Task UpdateAsync(T entity)
        {
            Update(entity);
            return Task.FromResult(0);
        }

        #endregion

        #region Reload/ReloadAsync

        public virtual void Reload(T entity)
        {
        }

        public virtual Task ReloadAsync(T entity)
        {
            Reload(entity);
            return Task.FromResult(0);
        }

        #endregion

        #region Fetch/FetchAsync

        public virtual IList<TResult> Fetch<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            return queryBuilder(m_InternalList.AsQueryable()).ToList();
        }

        public virtual Task<IList<TResult>> FetchAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder)
        {
            var list = Fetch(queryBuilder);
            return Task.FromResult<IList<TResult>>(list);
        }

        #endregion

        #region Find/FindAsync

        public virtual T Find(TKey id)
        {
            return this.Fetch(q => q.Where(entity => Equals(this.GetKey(entity), id))).SingleOrDefault();
        }

        public virtual Task<T> FindAsync(TKey id)
        {
            T entity = Find(id);
            return Task.FromResult<T>(entity);
        }

        #endregion
    }
}
