
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppBoot.Repos
{
    /// <summary> The base markup interface for all repository types. </summary>
    public interface IRepository
    {
        /// <summary> Gets <see cref="IRepositoryInfo"/>. </summary>
        IRepositoryInfo Info { get; }
    }

    /// <summary> Represents the repository for entity type <typeparamref name="T"/>. </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks> This interface enables defining extension methods 
    /// for both sync and async repositories. </remarks>
    public interface IRepository<T> : IRepository
    {
        /// <summary> Gets if one or more entities in this repository are inserted, updated, or deleted. </summary>
        bool HasChanges { get; }

        /// <summary> Adds an entity to the underlying data source. </summary>
        void Insert(T entity);

        /// <summary> Deletes an entity from the underlying data source. </summary>
        void Delete(T entity);

        /// <summary> Updates the underlying data source with an entity. </summary>
        void Update(T entity);

        /// <summary> Reloads an entity from the underlying data source. </summary>
        void Reload(T entity);

        /// <summary> Queries the underlying data source. </summary>
        /// <typeparam name="TResult"> The type of result. </typeparam>
        /// <param name="queryBuilder"> The <see cref="IQueryable{TResult}"/> of type <typeparamref name="TResult"/>. </param>
        /// <returns> An <see cref="IList{T}"/> of <typeparamref name="TResult"/>. </returns>
        /// <example>
        /// <code><![CDATA[
        /// IRepository<Account, Guid> repository = new AccountRepository(); 
        /// IList<Account> accounts = repository.Fetch(q => q.Where(account => account.Age > 30));
        /// ]]></code>
        /// </example>
        IList<TResult> Fetch<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder);

        /// <summary> Adds an entity to the underlying data source. </summary>
        Task InsertAsync(T entity);

        /// <summary> Deletes an entity from the underlying data source. </summary>
        Task DeleteAsync(T entity);

        /// <summary> Updates the underlying data source with an entity. </summary>
        Task UpdateAsync(T entity);

        /// <summary> Reloads an entity from the underlying data source. </summary>
        Task ReloadAsync(T entity);

        /// <summary> Queries the underlying data source. </summary>
        /// <typeparam name="TResult"> The type of result. </typeparam>
        /// <param name="queryBuilder"> The <see cref="IQueryable{TResult}"/> of type <typeparamref name="TResult"/>. </param>
        /// <returns> An <see cref="IList{T}"/> of <typeparamref name="TResult"/>. </returns>
        /// <example>
        /// <code><![CDATA[
        /// IAsyncRepository<Account, Guid> repository = new AccountRepository(); 
        /// IList<Account> accounts = await repository.FetchAsync(q => q.Where(account => account.Age > 30));
        /// ]]></code>
        /// </example>
        Task<IList<TResult>> FetchAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> queryBuilder);
    }

    public interface IRepository<T, TKey> : IRepository<T>
    {
        /// <summary> Finds an entity from a given primary key value. </summary>
        /// <returns> The entity for the given primary key value, or <c>null</c> if no such entity. </returns>
        T Find(TKey id);

        /// <summary> Finds an entity from a given primary key value. </summary>
        /// <returns> The entity for the given primary key value, or <c>null</c> if no such entity. </returns>
        Task<T> FindAsync(TKey id);
    }
}
