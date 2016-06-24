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
	/// <summary> ����һ��ϵͳ�Ľ�ɫ. </summary>
	public class RoleEntity : EntityBase<Guid>, IRole
	{
		/// <summary> ��ɫ��, �����ڵ�ǰ��ɫ����Ψһ. </summary>
		public String Name { get; set; }

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual byte[] RowVer { get; set; }

        #region Navigation Properties

        private readonly CollectionWrapper<AccountEntity, IAccount> m_Accounts 
            = new CollectionWrapper<AccountEntity, IAccount>(new HashSet<AccountEntity>());

		/// <summary> ������û� </summary>
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