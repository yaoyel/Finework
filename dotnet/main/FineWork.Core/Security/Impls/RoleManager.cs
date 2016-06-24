using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos;
using FineWork.Common;
using FineWork.Security.Repos;

namespace FineWork.Security.Impls
{
    public class RoleManager : IRoleManager
    {
        public RoleManager(IRoleRepository roleRepository, IAccountRepository accountRepository)
        {
            if (roleRepository == null) throw new ArgumentNullException("roleRepository");
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");

            m_RoleRepository = roleRepository;
            m_AccountRepository = accountRepository;
        }

        private readonly IRoleRepository m_RoleRepository;

        protected IRoleRepository RoleRepository
        {
            get { return m_RoleRepository; }
        }

        private readonly IAccountRepository m_AccountRepository;

        protected IAccountRepository AccountRepository
        {
            get { return m_AccountRepository; }
        }

        public IRole CreateRole(String roleName)
        {
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentNullException("roleName");

            var role = this.RoleRepository.CreateNew();
            role.Name = roleName;
            this.RoleRepository.Insert(role);
            return role;
        }

        public void DeleteRole(IRole role)
        {
            if (role == null) throw new ArgumentNullException("role");
            //TODO: throw if has members
            this.RoleRepository.Delete(role);
        }

        public IRole RenameRole(IRole role, String newRoleName)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (String.IsNullOrEmpty(newRoleName)) throw new ArgumentNullException("newRoleName");

            role.Name = newRoleName;
            this.RoleRepository.Update(role);
            
            return role;
        }

        public IRole FindRole(Guid id)
        {
            var role = this.RoleRepository.Find(id);
            return role;
        }

        public IRole FindRoleByName(String roleName)
        {
            if (String.IsNullOrEmpty(roleName)) throw new ArgumentNullException("roleName");
            var role = this.RoleRepository.Fetch(q => q.Where(x => x.Name == roleName)).SingleOrDefault();
            return role;
        }

        public ICollection<IRole> FetchRoles()
        {
            return this.RoleRepository.FetchAll();
        }

        public void GrantRole(IRole role, IAccount account)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (account == null) throw new ArgumentNullException("account");

            if (account.Roles.Any(x => x.Id == role.Id)) 
                throw new FineWorkException(String.Format(
                    "The role [{0} ({1})] has been granted to account [{2} {3}] already.",
                    role.Id, role.Name, account.Id, account.Name));
            role.Accounts.Add(account);
            this.RoleRepository.Update(role);
        }

        public void RevokeRole(IRole role, IAccount account)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (account == null) throw new ArgumentNullException("account");

            if (account.Roles.Any(x => x.Id == role.Id) == false)
                throw new FineWorkException(String.Format(
                    "The role [{0} ({1})] has not been granted to account [{2} {3}] already.",
                    role.Id, role.Name, account.Id, account.Name));
            role.Accounts.Remove(account);
            this.RoleRepository.Update(role);
        }

        public bool IsInRole(IRole role, IAccount account)
        {
            if (role == null) throw new ArgumentNullException("role");
            if (account == null) throw new ArgumentNullException("account");

            var rolesForAccount = this.RoleRepository.Fetch(
                x => x.Accounts.Any(a => a.Id == account.Id)
                );
            bool result = rolesForAccount.Count > 0;
            return result;
        }
    }
}
