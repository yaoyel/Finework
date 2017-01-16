using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IForumLikeManager
    {
        ForumLikeEntity CreateForumLike(Guid staffId, Guid topicId);

        void DeleteForumLike(Guid staffId, Guid topicId);

        IEnumerable<ForumLikeEntity> FetchForumLikesByTopicCreatorId(Guid staffId);

        IEnumerable<ForumLikeEntity> FetchForumLikesByTopicId(Guid topicId);
         

    }
}