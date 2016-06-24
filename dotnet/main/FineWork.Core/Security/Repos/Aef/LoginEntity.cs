using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Repos;
using FineWork.Common;

namespace FineWork.Security.Repos.Aef
{
    public class LoginEntity : EntityBase<Guid>, ILogin
    {
        public virtual String Provider { get; set; }

        public virtual String ProviderKey { get; set; }

        public virtual AccountEntity Account { get; set; }

        [NotMapped]
        IAccount ILogin.Account
        {
            get { return this.Account; }
            set { this.Account = (AccountEntity)value; }
        }

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual byte[] RowVer { get; set; }
    }
}