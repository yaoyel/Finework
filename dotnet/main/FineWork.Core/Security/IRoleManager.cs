using System;
using System.Collections.Generic;

namespace FineWork.Security
{
    public interface IRoleManager
    {
        IRole CreateRole(String roleName);

        void DeleteRole(IRole role);

        IRole RenameRole(IRole role, String newRoleName);

        IRole FindRole(Guid id);

        IRole FindRoleByName(String roleName);

        ICollection<IRole> FetchRoles();

        void GrantRole(IRole role, IAccount account);

        void RevokeRole(IRole role, IAccount account);

        bool IsInRole(IRole role, IAccount account);
    }
}
