using System;
using AppBoot.Repos;
using AppBoot.Repos.Adapters;
using AppBoot.Repos.Aef;

namespace FineWork.Security.Repos.Aef
{
    public class ClaimRepository
        : AefUpCastRepository<IClaim, Guid, ClaimEntity>
        , IClaimRepository
    {
        public ClaimRepository(ISessionProvider<AefSession> session)
            : base(session)
        {
        }

        public IClaim CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IClaim Create(Guid id)
        {
            return new ClaimEntity {Id = id};
        }
    }
}
