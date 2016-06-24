using System;
using AppBoot.Repos.Inmem;
using FineWork.Security.Repos.Aef;

namespace FineWork.Security.Repos.Fakes
{
    public class FakeRoleRepository : InmemUpCastRepository<IRole, Guid, RoleEntity>, IRoleRepository
    {
        public FakeRoleRepository()
            : base(entity => entity.Id)
        {
        }

        public IRole CreateNew()
        {
            return Create(Guid.NewGuid());
        }

        public IRole Create(Guid id)
        {
            return new RoleEntity {Id=id};
        }
    }
}