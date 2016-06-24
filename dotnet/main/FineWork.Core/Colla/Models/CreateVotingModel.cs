using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateVotingModel
    { 
        public Guid StaffId { get; set; }

        public ICollection<VotingModel> Votings { get; set; } 
    }

    public class VotingModel
    {
        public Guid VoteOptionId { get; set; }
        public string Reason { get; set; }

    }
}
