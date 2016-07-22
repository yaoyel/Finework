using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class VoteOptionEntity:EntityBase<Guid>
    {
        public string Content { get; set; }

        public bool IsNeedReason { get; set; } = false;

        public int Order { get; set; }

        public virtual ICollection<VotingEntity> Votings { get; set; } = new HashSet<VotingEntity>(); 

        public VoteEntity Vote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}
