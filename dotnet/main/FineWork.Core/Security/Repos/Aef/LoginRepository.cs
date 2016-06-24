using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppBoot.KeyGenerators;
using AppBoot.Repos;
using AppBoot.Repos.Adapters;
using AppBoot.Repos.Aef;

namespace FineWork.Security.Repos.Aef
{
    public class LoginRepository
        : AefUpCastRepository<ILogin, Guid, LoginEntity>
        , ILoginRepository
    {
        public LoginRepository(ISessionProvider<AefSession> session)
            : base(session)
        {
        }

        public ILogin Find(String loginProvider, String loginProviderKey)
        {
            Expression<Func<ILogin, bool>> filter = x => x.Provider == loginProvider && x.ProviderKey == loginProviderKey;
            ILogin login = this.Fetch(q => q.Where(filter)).SingleOrDefault();
            return login;
        }

        public async Task<ILogin> FindAsync(string loginProvider, string loginProviderKey)
        {
            Expression<Func<ILogin, bool>> filter = x => x.Provider == loginProvider && x.ProviderKey == loginProviderKey;
            ILogin login = (await this.FetchAsync(q => q.Where(filter))).SingleOrDefault();
            return login;
        }

        public IAccount FindAccount(String loginProvider, String loginProviderKey)
        {
            ILogin login = Find(loginProvider, loginProviderKey);
            return login != null ? login.Account : null;
        }

        public async Task<IAccount> FindAccountAsync(string loginProvider, string loginProviderKey)
        {
            var login = await FindAsync(loginProvider, loginProviderKey);
            return login != null ? login.Account : null;
        }

        public ILogin CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public ILogin Create(Guid id)
        {
            return new LoginEntity { Id = id };
        }
    }
}
