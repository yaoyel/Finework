using System;
using System.Collections.Generic;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;

namespace FineWork.Colla.Impls
{
    public class ForumCommentManager : AefEntityManager<ForumCommentEntity, Guid>, IForumCommentManager
    {
        public ForumCommentManager(ISessionProvider<AefSession> sessionProvider,
            IForumTopicManager forumTopicManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(forumTopicManager, nameof(forumTopicManager));
            Args.NotNull(staffManager, nameof(staffManager));

            m_ForumTopicManager = forumTopicManager;
            m_StaffManager = staffManager;
        }

        private readonly IForumTopicManager m_ForumTopicManager;
        private readonly IStaffManager m_StaffManager;

        public ForumCommentEntity CreateForumComment(CraeteForumCommentModel forumCommentModel)
        {
            Args.NotNull(forumCommentModel, nameof(forumCommentModel));

            ForumCommentEntity targetComment = null;

            var topic =
                ForumTopicExistsResult.Check(this.m_ForumTopicManager, forumCommentModel.TopicId)
                    .ThrowIfFailed()
                    .ForumTopic;

            if (forumCommentModel.TargetCommentId != default(Guid))
                targetComment =
                    ForumCommentExistsResult.Check(this, forumCommentModel.TargetCommentId).ThrowIfFailed().ForumComment;

            var staff = StaffExistsResult.Check(this.m_StaffManager, forumCommentModel.StaffId).ThrowIfFailed().Staff;

            var forumComment = new ForumCommentEntity();
            forumComment.TargetComment = targetComment;
            forumComment.TargetContent = targetComment?.Content;
            forumComment.Content = forumCommentModel.Comment;
            forumComment.Staff = staff;
            forumComment.ForumTopic = topic;
            forumComment.Id = Guid.NewGuid();

            this.InternalInsert(forumComment);
            return forumComment;
        }

        public IEnumerable<ForumCommentEntity> FetchForumCommentsByTopicId(Guid topicId)
        {
            return this.InternalFetch(p => p.ForumTopic.Id == topicId);
        }

        public ForumCommentEntity FindById(Guid forumCommentId)
        {
            return this.InternalFind(forumCommentId);
        }

        public void DeleteForumComment(Guid commentId)
        {
            var comment = ForumCommentExistsResult.Check(this, commentId).ForumComment;

            if(comment!=null) this.InternalDelete(comment);

        }

        public ForumCommentEntity UpdateForumComment(UpdateForumCommentModel updateForumCommentModel)
        {
            Args.NotNull(updateForumCommentModel, nameof(updateForumCommentModel));

            var comment = ForumCommentExistsResult.Check(this, updateForumCommentModel.CommentId).ThrowIfFailed().ForumComment;

            var staff =
                StaffExistsResult.Check(this.m_StaffManager, updateForumCommentModel.StaffId).ThrowIfFailed().Staff;

            if(comment.Staff!=staff) throw new FineWorkException("你没有权限修改此评论");

            comment.Content = updateForumCommentModel.Comment;
            comment.LastUpdatedAt=DateTime.Now; 
            
            this.InternalUpdate(comment);
            return comment; 
        }
    }
}