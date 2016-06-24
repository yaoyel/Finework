using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Models;
using JetBrains.Annotations;

namespace FineWork.Colla.Impls
{
    internal class VoteOptioinManager : AefEntityManager<VoteOptionEntity, Guid>, IVoteOptionManager
    {
        internal VoteOptioinManager(ISessionProvider<AefSession> sessionProvider)
            : base(sessionProvider)
        {
        }

        public VoteOptionEntity CreateVoteOption([NotNull]VoteEntity vote,CreateVoteOptionModel voteOptionModel)
        { 
            var voteOption = new VoteOptionEntity()
            {
                Id=Guid.NewGuid(),
                Content=voteOptionModel.Content,
                IsNeedReason=voteOptionModel.IsNeedReason,
                Order=voteOptionModel.Order,
                Vote=vote
            };

            this.InternalInsert(voteOption);
            return voteOption;
        }

        public IEnumerable<VoteOptionEntity> FetchVoteOptionsByTaskIdAndVoteId(Guid taskId, Guid voteId)
        {
            return this.InternalFetch(p => p.Vote.Task.Id == taskId && p.Vote.Id == voteId)
                .OrderBy(p=>p.Order);
        }

        public VoteOptionEntity FindVoteOptionByOptionId(Guid optionId)
        {
            return this.InternalFind(optionId);
        }
    }
}
