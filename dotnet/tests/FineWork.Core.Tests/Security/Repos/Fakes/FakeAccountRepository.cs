using System;
using AppBoot.Repos;
using AppBoot.Repos.Inmem;
using FineWork.Security.Repos.Aef;

namespace FineWork.Security.Repos.Fakes
{
    public class FakeAccountRepository : InmemUpCastRepository<IAccount, Guid, AccountEntity>, IAccountRepository
    {
        public FakeAccountRepository()
            : base(entity => entity.Id)
        {
        }

        public IAccount CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IAccount Create(Guid id)
        {
            return new AccountEntity { Id = id };
        }
    }
}