using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumVoteManager
    {
        ForumVoteEntity CreateForumVote(VoteEntity vote, ForumTopicEntity topic);

        ForumVoteEntity FindByVoteId(Guid voteId);

        IEnumerable<ForumVoteEntity> FetchVotesByTopicId(Guid topicId);
    }
}