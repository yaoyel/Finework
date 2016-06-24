using System;
using AppBoot.Repos;

namespace FineWork.Security.Repos
{
    public interface IRoleRepository : IRepository<IRole, Guid>
    {
        IRole CreateNew();

        IRole Create(Guid id);
    }
}