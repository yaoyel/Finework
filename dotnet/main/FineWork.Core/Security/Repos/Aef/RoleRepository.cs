using System;
using AppBoot.Repos;
using AppBoot.Repos.Aef;

namespace FineWork.Security.Repos.Aef
{
    /// <summary> The repository for <see cref="RoleEntity"/>. </summary>
    public class RoleRepository 
        : AefUpCastRepository<IRole, Guid, RoleEntity> 
        , IRoleRepository
    {
        public RoleRepository(ISessionProvider<AefSession> session)
            : base(session)
        {
        }

        public IRole CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IRole Create(Guid id)
        {
            return new RoleEntity { Id = id };
        }
    }
}
