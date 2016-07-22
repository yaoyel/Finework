using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IForumCommentLikeManager
    {
        ForumCommentLikeEntity CreateForumComentLike(Guid staffId, Guid commentId);

        void DeleteForumCommentLike(Guid staffId, Guid commentId);

        IEnumerable<ForumCommentLikeEntity> FetchCommentLikesByCommentId(Guid topicId);
    }
}