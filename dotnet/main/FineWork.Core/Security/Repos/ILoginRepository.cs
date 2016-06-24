using System;
using System.Threading.Tasks;
using AppBoot.Repos;

namespace FineWork.Security.Repos
{
    public interface ILoginRepository : IRepository<ILogin, Guid>
    {
        /// <summary> Finds the login by the provider and the provider key. </summary>
        /// <returns> The <see cref="ILogin"/> if found, otherwise <c>null</c>. </returns>
        ILogin Find(String loginProvider, String loginProviderKey);

        /// <summary> Finds the login by the provider and the provider key asynchronically. </summary>
        /// <returns> The <see cref="ILogin"/> if found, otherwise <c>null</c>. </returns>
        Task<ILogin> FindAsync(String loginProvider, String loginProviderKey);

        /// <summary> Finds the account by a login. </summary>
        /// <returns> The <see cref="IAccount"/> if found, otherwise <c>null</c>. </returns>
        IAccount FindAccount(String loginProvider, String loginProviderKey);


        /// <summary> Finds the account by a login asynchronically. </summary>
        /// <returns> The <see cref="IAccount"/> if found, otherwise <c>null</c>. </returns>
        Task<IAccount> FindAccountAsync(String loginProvider, String loginProviderKey);

        ILogin CreateNew();

        ILogin Create(Guid id);
    }
}
