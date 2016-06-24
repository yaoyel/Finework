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
    /// <summary> 代表一个用户 </summary>
    public class AccountEntity : EntityBase<Guid>, IAccount
    {
        public String Name { get; set; }

        public String Password { get; set; }

        public String PasswordSalt { get; set; }

        public String PasswordFormat { get; set; }

        public String Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public String PhoneNumber { get; set; }

        public bool IsPhoneNumberConfirmed { get; set; }

        public bool IsTwoFactorEnabled { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockEndAt { get; set; }

        public int PasswordFailedCount { get; set; }

        public String SecurityStamp { get; set; }

        public DateTime CreatedAt { get; set; } 

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual Byte[] RowVer { get; set; }

        #region Navigation Properties

        #region Roles

        private readonly CollectionWrapper<RoleEntity, IRole> m_Roles 
            = new CollectionWrapper<RoleEntity, IRole>(new HashSet<RoleEntity>());

        /// <summary> 拥有的角色. </summary>
        public virtual ICollection<RoleEntity> Roles
        {
            get { return m_Roles.Source; }
            set { m_Roles.Source = value; }
        }

        [NotMapped]
        ICollection<IRole> IAccount.Roles
        {
            get { return m_Roles; }
        }

        #endregion

        #region Logins

        private readonly CollectionWrapper<LoginEntity, ILogin> m_Logins
            = new CollectionWrapper<LoginEntity, ILogin>(new HashSet<LoginEntity>());

        public virtual ICollection<LoginEntity> Logins
        {
            get { return m_Logins.Source; }
            set { m_Logins.Source = value; }
        }

        [NotMapped]
        ICollection<ILogin> IAccount.Logins
        {
            get { return m_Logins; }
        }

        #endregion

        #region Claims

        private readonly CollectionWrapper<ClaimEntity, IClaim> m_Claims
            = new CollectionWrapper<ClaimEntity, IClaim>(new HashSet<ClaimEntity>());

        public virtual ICollection<ClaimEntity> Claims
        {
            get { return m_Claims.Source; }
            set { m_Claims.Source = value; }
        }

        [NotMapped]
        ICollection<IClaim> IAccount.Claims
        {
            get { return m_Claims; }
        }

        #endregion

        #endregion
    }
}
