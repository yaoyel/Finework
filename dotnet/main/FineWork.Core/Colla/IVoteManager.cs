using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IVoteManager
    {
        VoteEntity CreateVote(CreateVoteModel voteModel);
         
        IEnumerable<VoteEntity> FetchVotesByTaskId(Guid taskId,bool? isApproved);

        VoteEntity FindVoteByVoteId(Guid voteId);

        IEnumerable<VoteEntity> FetchVotesByStaffId(Guid staffId);
          
        void UpdateVoteApprovedStatus(VoteEntity vote,bool isApproved);

        void UpdateVote(UpdateVoteModel voteModel); 

    }
}
