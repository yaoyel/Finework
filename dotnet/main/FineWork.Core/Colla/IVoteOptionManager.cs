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
        

        VoteOptionEntity FindVoteOptionByOptionId(Guid optionId);

        void UpdateVoteOption(UpdateVoteOptionModel voteOptionModel);
    }
}
