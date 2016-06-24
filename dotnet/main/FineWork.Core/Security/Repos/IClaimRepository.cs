using System;
using AppBoot.Repos;
using JetBrains.Annotations;

namespace FineWork.Security.Repos
{
    public interface IClaimRepository : IRepository<IClaim, Guid>
    {
        IClaim CreateNew();

        IClaim Create(Guid id);
    }
}
