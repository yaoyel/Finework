using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumCommentManager
    {
        ForumCommentEntity CreateForumComment(CraeteForumCommentModel forumComment);

        IEnumerable<ForumCommentEntity> FetchForumCommentsByTopicId(Guid topicId);

        IEnumerable<ForumCommentEntity> FetchCommentsByTopicOrCommentCreator(Guid staffId);

        void DeleteForumComment(Guid commentId);

        ForumCommentEntity FindById(Guid forumCommentId);

        ForumCommentEntity UpdateForumComment(UpdateForumCommentModel updateForumCommentModel);
    }
}