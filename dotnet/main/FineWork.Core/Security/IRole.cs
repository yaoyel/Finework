using System;
using System.Collections.Generic;
using AppBoot.Repos;

namespace FineWork.Security
{
    /// <summary> 代表一个系统的角色. </summary>
    /// <remarks> <para> 参见: http://dev-forge.chinahrd.net/redmine/projects/emall/wiki/角色 </para> </remarks>
    public interface IRole
    {
        Guid Id { get; }

        String Name { get; set; }

        #region Navigation Properties

        /// <summary> 授予的用户 </summary>
        /// <seealso cref="IAccount.Roles"/>
        ICollection<IAccount> Accounts { get; }

        #endregion
    }
}