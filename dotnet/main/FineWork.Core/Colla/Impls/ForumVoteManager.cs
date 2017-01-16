using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;

namespace FineWork.Colla.Impls
{
    public class ForumVoteManager: AefEntityManager<ForumVoteEntity, Guid>,IForumVoteManager
    {
        public ForumVoteManager(ISessionProvider<AefSession> sessionProvider,
            IForumTopicManager forumTopicManager,
            IVoteManager voteManger) : base(sessionProvider)
        {
            Args.NotNull(forumTopicManager, nameof(forumTopicManager));
            Args.NotNull(voteManger, nameof(voteManger));

            m_ForumTopicManager = forumTopicManager;
            m_VoteManager = voteManger;
        }


        private readonly IForumTopicManager m_ForumTopicManager;
        private readonly IVoteManager m_VoteManager;
        public ForumVoteEntity CreateForumVote(VoteEntity vote,ForumTopicEntity topic)
        {
            Args.NotNull(vote, nameof(vote));
            Args.NotNull(topic, nameof(topic));  

            var forumVote=new ForumVoteEntity();
            forumVote.Id = Guid.NewGuid();
            forumVote.Vote = vote;
            forumVote.ForumTopic = topic; 

            this.InternalInsert(forumVote);
            return forumVote;

        }

        public ForumVoteEntity FindByVoteId(Guid voteId)
        {
            return this.InternalFetch(p => p.Vote.Id == voteId).FirstOrDefault();
        }

        public IEnumerable<ForumVoteEntity> FetchVotesByTopicId(Guid topicId)
        {
            return this.InternalFetch(p => p.ForumTopic.Id == topicId);
        }
    }
}