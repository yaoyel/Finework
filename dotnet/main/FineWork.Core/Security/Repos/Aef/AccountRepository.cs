using System;
using AppBoot.Repos;
using AppBoot.Repos.Adapters;
using AppBoot.Repos.Aef;

namespace FineWork.Security.Repos.Aef
{
    /// <summary> The repository for <see cref="AccountEntity"/>. </summary>
    public class AccountRepository 
        : AefUpCastRepository<IAccount, Guid, AccountEntity>
        , IAccountRepository
    {
        public AccountRepository(ISessionProvider<AefSession> session)
            : base(session)
        {
        }

        public IAccount CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IAccount Create(Guid id)
        {
            return new AccountEntity {Id = id};
        }
    }
}
