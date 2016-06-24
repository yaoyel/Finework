using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IVoteOptionManager
    {
        VoteOptionEntity CreateVoteOption(VoteEntity vote, CreateVoteOptionModel voteOptionModel);

        IEnumerable<VoteOptionEntity> FetchVoteOptionsByTaskIdAndVoteId(Guid taskId, Guid voteId);

        VoteOptionEntity FindVoteOptionByOptionId(Guid optionId);
    }
}
