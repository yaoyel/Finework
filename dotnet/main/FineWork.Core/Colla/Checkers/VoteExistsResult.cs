using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class VoteExistsResult: FineWorkCheckResult
    {
        public VoteExistsResult(bool isSucceed, String message, VoteEntity vote)
            : base(isSucceed, message)
        {
            this.Vote = vote;
        }

        public VoteEntity Vote { get; private set; }

        public static VoteExistsResult Check(IVoteManager voteManager, Guid voteId)
        {
            var  vote = voteManager.FindVoteByVoteId(voteId); 
            return Check(vote, "不存在对应的共识信息.");
        }

        private static VoteExistsResult Check([CanBeNull] VoteEntity vote, String message)
        {
            if (vote == null)
            {
                return new VoteExistsResult(false, message, null);
            }
            return new VoteExistsResult(true, null, vote);
        }
    }
}
