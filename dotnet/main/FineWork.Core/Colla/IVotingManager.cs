using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IVotingManager
    {
        IEnumerable<VotingEntity> CreateVoting(CreateVotingModel votingModel);

        IEnumerable<VotingEntity> FetchVotingByVoteId(Guid voteId);

        IEnumerable<VotingEntity> FetchVotingByVoteOptionId(Guid voteOptionId);

        IEnumerable<VotingEntity> FetchVotingByTaskIdAndStaffId(Guid taskId, Guid staffId);

        VotingEntity FindByOptionIdWithStaffId(Guid optionId, Guid staffId);

        void DeleteVotingByVoteId(VoteEntity vote, Guid staffId);
    }
}
