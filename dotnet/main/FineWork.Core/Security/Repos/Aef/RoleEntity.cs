using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Core;
using FineWork.Common;

namespace FineWork.Security.Repos.Aef
{
	/// <summary> 代表一个系统的角色. </summary>
	public class RoleEntity : EntityBase<Guid>, IRole
	{
		/// <summary> 角色名, 必须在当前角色域中唯一. </summary>
		public String Name { get; set; }

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual byte[] RowVer { get; set; }

        #region Navigation Properties

        private readonly CollectionWrapper<AccountEntity, IAccount> m_Accounts 
            = new CollectionWrapper<AccountEntity, IAccount>(new HashSet<AccountEntity>());

		/// <summary> 授予的用户 </summary>
		/// <seealso cref="AccountEntity.Roles"/>
		public virtual ICollection<AccountEntity> Accounts
		{
            get { return m_Accounts.Source; }
            set { m_Accounts.Source = value; }
		}
            
        [NotMapped]
		ICollection<IAccount> IRole.Accounts
		{
            get { return m_Accounts; }
		}

        #endregion
	}
}