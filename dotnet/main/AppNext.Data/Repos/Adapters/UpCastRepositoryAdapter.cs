using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Core;

namespace AppBoot.Repos.Adapters
{
    /// <summary> Adaptes <see cref="IRepository{T,TKey}"/> with generic parameter 
    /// &lt;<typeparamref name="TImpl"/>, <typeparamref name="TKey"/>&gt;
    /// to <see cref="IRepository{T,TKey}"/> with generic parameter 
    /// &lt;<typeparamref name="TDecl"/>, <typeparamref name="TKey"/>&gt;. </summary>
    /// <remarks> <see cref="UpCastRepositoryAdapter{TDecl,TKey,TImpl,TImplRepository}"/> 
    /// is used to support defining entity as an interface or an abstract class type. 
    /// <para>Programmers can seperate the entity declaraction as 
    /// an interface or an abstract class from its implementation concrete class 
    /// to archive maximun extensibility. 
    /// <see cref="UpCastRepositoryAdapter{TDecl,TKey,TImpl,TImplRepository}"/> delegates
    /// repository operations with the declaration type to 
    /// <typeparamref name="TImplRepository"/> which operates on the implementation type.</para>
    /// </remarks>
    public class UpCastRepositoryAdapter<TDecl, TKey, TImpl, TImplRepository>
        : IRepository<TDecl, TKey>
        where TDecl : class
        where TImpl : class, TDecl
        where TImplRepository : IRepository<TImpl, TKey>
    {
        public UpCastRepositoryAdapter(TImplRepository adaptedRepository)
        {
            if (Object.Equals(adaptedRepository, null)) throw new ArgumentNullException("adaptedRepository");

            m_AdaptedRepository = adaptedRepository;
        }

        private readonly TImplRepository m_AdaptedRepository;

        public TImplRepository AdaptedRepository
        {
            get { return m_AdaptedRepository; }
        }

        public virtual IRepositoryInfo Info
        {
            get { return this.AdaptedRepository.Info; }
        }

        public virtual void Insert(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            this.AdaptedRepository.Insert(entityImpl);
        }

        public virtual void Delete(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            this.AdaptedRepository.Delete(entityImpl);
        }

        public virtual void Update(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            this.AdaptedRepository.Update(entityImpl);
        }

        public virtual void Reload(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            this.AdaptedRepository.Reload(entityImpl);
        }

        public virtual TDecl Find(TKey id)
        {
            return this.AdaptedRepository.Find(id);
        }

        public virtual IList<TResult> Fetch<TResult>(Func<IQueryable<TDecl>, IQueryable<TResult>> queryBuilder)
        {
            return this.AdaptedRepository.Fetch(queryBuilder);
        }

        public virtual bool HasChanges
        {
            get { return this.AdaptedRepository.HasChanges; }
        }

        public virtual async Task InsertAsync(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            await this.AdaptedRepository.InsertAsync(entityImpl);
        }

        public virtual async Task DeleteAsync(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            await this.AdaptedRepository.DeleteAsync(entityImpl);
        }

        public virtual async Task UpdateAsync(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            await this.AdaptedRepository.UpdateAsync(entityImpl);
        }

        public virtual async Task ReloadAsync(TDecl entity)
        {
            var entityImpl = this.AdaptedRepository.CastEntity(entity);
            await this.AdaptedRepository.ReloadAsync(entityImpl);
        }

        public virtual async Task<TDecl> FindAsync(TKey id)
        {
            return await this.AdaptedRepository.FindAsync(id);
        }

        public virtual async Task<IList<TResult>> FetchAsync<TResult>(Func<IQueryable<TDecl>, IQueryable<TResult>> queryBuilder)
        {
            return await this.AdaptedRepository.FetchAsync(queryBuilder);
        }

    }
}
