using System;
using System.Runtime.InteropServices.ComTypes;
using AppBoot.Repos;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Repos
{
    public static class RepoTestUtil
    {
        public static void CheckRepositoryRegistered<TRepository, T, TImpl, TKey>(this IServiceProvider serviceProvider)
            where TRepository : class, IRepository<T, TKey>
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            var repository = serviceProvider.GetService<TRepository>();
            Assert.NotNull(repository, "The repository of type [{0}] has not been registered.", typeof(TRepository).FullName);
            Assert.IsInstanceOf<IRepository<T, TKey>>(repository);
            Assert.AreSame(typeof(TImpl), repository.Info.EntityType);
            Assert.AreSame(typeof(TKey), repository.Info.KeyType);
        }
    }
}
