using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class VoteEntity : EntityBase<Guid>
    {
        public string Subject { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public bool IsMultiEnabled { get; set; } = false;

        public bool IsAnonEnabled { get; set; } = false;

        /// <summary>
        /// 为null是为未投票，true为已共识，false为未共识
        /// </summary>
        public bool? IsApproved { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual StaffEntity Creator { get; set; }

        public virtual TaskEntity Task { get; set; } 

        public virtual ICollection<VoteOptionEntity> VoteOptions { get; set; } = new HashSet<VoteOptionEntity>();
    }
}
