using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AppBoot.Repos
{
    public static class RepositoryExtensions
    {
        public static IList<T> Fetch<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            return repository.Fetch<T>(q => q.Where(predicate));
        }

        public static Task<IList<T>> FetchAsync<T>(this IRepository<T> repository, Expression<Func<T, bool>> predicate)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            return repository.FetchAsync<T>(q => q.Where(predicate));
        }

        public static IList<T> FetchAll<T>(this IRepository<T> repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            return repository.Fetch<T>(q => q);
        }

        public static Task<IList<T>> FetchAllAsync<T>(this IRepository<T> repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            return repository.FetchAsync<T>(q => q);
        }
    }
}