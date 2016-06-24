using System;
using AppBoot.Repos;
using AppBoot.Repos.Inmem;
using FineWork.Security.Repos.Aef;

namespace FineWork.Security.Repos.Fakes
{
    public class FakeClaimRepository : InmemUpCastRepository<IClaim, Guid, ClaimEntity>, IClaimRepository
    {
        public FakeClaimRepository()
            : base(entity => entity.Id)
        {
        }

        public IClaim CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IClaim Create(Guid id)
        {
            return new ClaimEntity { Id = id };
        }
    }
}