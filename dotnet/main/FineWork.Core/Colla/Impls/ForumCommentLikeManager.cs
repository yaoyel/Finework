using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class ForumCommentLikeManager : AefEntityManager<ForumCommentLikeEntity, Guid>, IForumCommentLikeManager
    {
        public ForumCommentLikeManager(ISessionProvider<AefSession> sessionProvider,
            IForumCommentManager forumCommentManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(forumCommentManager, nameof(forumCommentManager));
            Args.NotNull(staffManager, nameof(staffManager));

            m_ForumCommentManager = forumCommentManager;
            m_StaffManager = staffManager;
        }

        private readonly IForumCommentManager m_ForumCommentManager;
        private readonly IStaffManager m_StaffManager;
        public ForumCommentLikeEntity CreateForumComentLike(Guid staffId, Guid commentId)
        {
            var comment =
                ForumCommentExistsResult.Check(this.m_ForumCommentManager, commentId).ThrowIfFailed().ForumComment;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var commentLike = new  ForumCommentLikeEntity();
            commentLike.Staff = staff;
            commentLike.ForumComment= comment;
            commentLike.Id = Guid.NewGuid();

            this.InternalInsert(commentLike);
            return commentLike;
        }

 
        public void DeleteForumCommentLike(Guid staffId, Guid commentId)
        {
            var forumLike =
                this.InternalFetch(p => p.Staff.Id == staffId && p.ForumComment.Id == commentId).FirstOrDefault();

            if (forumLike != null) this.InternalDelete(forumLike);
        } 
 
        public IEnumerable<ForumCommentLikeEntity> FetchCommentLikesByCommentId(Guid commentId)
        {
            return this.InternalFetch(p => p.ForumComment.Id == commentId);
        }
    }
}