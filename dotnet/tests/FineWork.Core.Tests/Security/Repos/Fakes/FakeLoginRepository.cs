using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Inmem;
using FineWork.Security.Repos.Aef;

namespace FineWork.Security.Repos.Fakes
{
    public class FakeLoginRepository : InmemUpCastRepository<ILogin, Guid, LoginEntity>, ILoginRepository
    {
        public FakeLoginRepository()
            : base(entity => entity.Id)
        {
        }

        public ILogin CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public ILogin Create(Guid id)
        {
            return new LoginEntity { Id = id };
        }

        public ILogin Find(string loginProvider, string loginProviderKey)
        {
            Expression<Func<ILogin, bool>> filter =
                l => l.Provider == loginProvider && l.ProviderKey == loginProviderKey;
            return this.Fetch(filter).SingleOrDefault();
        }

        public Task<ILogin> FindAsync(string loginProvider, string loginProviderKey)
        {
            var login = Find(loginProvider, loginProviderKey);
            return Task.FromResult<ILogin>(login);
        }

        public IAccount FindAccount(string loginProvider, string loginProviderKey)
        {
            ILogin login = Find(loginProvider, loginProviderKey);
            return login != null ? login.Account : null;
        }

        public Task<IAccount> FindAccountAsync(string loginProvider, string loginProviderKey)
        {
            var account = FindAccount(loginProvider, loginProviderKey);
            return Task.FromResult<IAccount>(account);
        }
    }
}