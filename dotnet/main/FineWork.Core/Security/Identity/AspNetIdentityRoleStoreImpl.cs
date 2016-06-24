using System;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using FineWork.Security.Repos;
using Microsoft.AspNet.Identity;

namespace FineWork.Security.Identity
{
    public class AspNetIdentityRoleStoreImpl : DisposableBase, IRoleStore<IRole, Guid>
    {
        public AspNetIdentityRoleStoreImpl(IRoleRepository roleRepository)
        {
            if (roleRepository == null) throw new ArgumentNullException("roleRepository");
            this.m_RoleRepository = roleRepository;
        }

        private readonly IRoleRepository m_RoleRepository;

        protected IRoleRepository RoleRepository
        {
            get { return m_RoleRepository; }
        }

        public Task CreateAsync(IRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            return this.RoleRepository.InsertAsync(role);
        }

        public Task UpdateAsync(IRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            return this.RoleRepository.UpdateAsync(role);
        }

        public Task DeleteAsync(IRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            return this.RoleRepository.DeleteAsync(role);
        }

        public Task<IRole> FindByIdAsync(Guid roleId)
        {
            return this.RoleRepository.FindAsync(roleId);
        }

        public async Task<IRole> FindByNameAsync(String roleName)
        {
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentException("roleName is null or empty.", "roleName");
            var list = await this.RoleRepository.FetchAsync(x => x.Name == roleName);
            return list.SingleOrDefault();
        }
    }
}