using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using JetBrains.Annotations;

namespace FineWork.Colla.Impls
{
    internal class VoteOptioinManager : AefEntityManager<VoteOptionEntity, Guid>, IVoteOptionManager
    {
        internal VoteOptioinManager(ISessionProvider<AefSession> sessionProvider,
            ITaskVoteManager taskVoteManager)
            : base(sessionProvider)
        {
            Args.NotNull(taskVoteManager, nameof(taskVoteManager));
            m_TaskVoteManager = taskVoteManager;
        }

        private readonly ITaskVoteManager m_TaskVoteManager;

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


        public VoteOptionEntity FindVoteOptionByOptionId(Guid optionId)
        {
            return this.InternalFind(optionId);
        }

        public void UpdateVoteOption(UpdateVoteOptionModel voteOptionModel)
        {
            Args.NotNull(voteOptionModel, nameof(voteOptionModel));

            var option = VoteOptionExistsResult.Check(this, voteOptionModel.OptionId).ThrowIfFailed().VoteOption;

            option.Content = voteOptionModel.Content;
            option.IsNeedReason = voteOptionModel.IsNeedReason;

            this.InternalUpdate(option);
        }
    }
}
