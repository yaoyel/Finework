using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Repos;
using FineWork.Common;

namespace FineWork.Security.Repos.Aef
{
    /// <summary>
    /// EntityType that represents one specific user claim
    /// 
    /// </summary>
    public class ClaimEntity : EntityBase<Guid>, IClaim
    {
        /// <summary>
        /// Claim type
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Claim value
        /// </summary>
        public virtual string ClaimValue { get; set; }

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual byte[] RowVer { get; set; }


        #region Navigation Properties

        public virtual AccountEntity Account { get; set; }

        [NotMapped]
        IAccount IClaim.Account
        {
            get { return this.Account; }
            set { this.Account = (AccountEntity) value; }
        }

        #endregion
    }
}
