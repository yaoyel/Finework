using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppBoot.Repos
{
    /// <summary> <see cref="ISession"/> represents a data context, such as <c>DbContext</c> in EF. </summary>
    public interface ISession : IDisposable
    {
        /// <summary> Saves changes to the underlying database. </summary>
        /// <exception cref="ObjectDisposedException"> if the session has been disposed. </exception>
        int SaveChanges();

        /// <summary> Saves changes to the underlying database asynchronously. </summary>
        /// <exception cref="ObjectDisposedException"> if the session has been disposed. </exception>
        Task<int> SaveChangesAsync(CancellationToken token);

        Task SaveChangesAsync();
    }
}