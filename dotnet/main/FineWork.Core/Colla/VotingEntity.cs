using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class VotingEntity:EntityBase<Guid>  
    {
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual VoteOptionEntity Option { get; set; }

        public virtual StaffEntity Staff { get; set; }
         
    }
}
