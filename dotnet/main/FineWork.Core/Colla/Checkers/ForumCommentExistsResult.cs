using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class ForumCommentExistsResult : FineWorkCheckResult
    {
        public ForumCommentExistsResult(bool isSucceed, String message, ForumCommentEntity forumComment)
            : base(isSucceed, message)
        {
            this.ForumComment = forumComment;
        }

        public ForumCommentEntity ForumComment { get; set; }
        public static ForumCommentExistsResult Check(IForumCommentManager forumCommentManager, Guid forumCommentId)
        {
            var forumComment = forumCommentManager.FindById(forumCommentId);
            return Check(forumComment, "不存在对应的评论.");
        }


        private static ForumCommentExistsResult Check([CanBeNull]  ForumCommentEntity forumComment, String message)
        {
            if (forumComment == null)
            {
                return new ForumCommentExistsResult(false, message, null);
            }

            return new ForumCommentExistsResult(true, null, forumComment);
        }
    }
}
