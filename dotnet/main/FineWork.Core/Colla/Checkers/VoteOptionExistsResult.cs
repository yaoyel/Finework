using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class VoteOptionExistsResult: FineWorkCheckResult
    {
        public VoteOptionExistsResult(bool isSucceed, String message, VoteOptionEntity voteOption)
            : base(isSucceed, message)
        {
            this.VoteOption = voteOption;
        }

        public VoteOptionEntity VoteOption { get; private set; }

        public static VoteOptionExistsResult Check(IVoteOptionManager voteOptionManager, Guid voteOptionId)
        {
            if (voteOptionManager == null) throw new ArgumentException(nameof(voteOptionManager));

            var voteOption = voteOptionManager.FindVoteOptionByOptionId(voteOptionId); 
            return Check(voteOption, "不存在对应的共识选项.");
        }

        private static VoteOptionExistsResult Check(VoteOptionEntity voteOption, String message)
        {
            if (voteOption == null)
            {
                return new VoteOptionExistsResult(false, message, null);
            }
            return new VoteOptionExistsResult(true, null, voteOption);
        }
    }
}
