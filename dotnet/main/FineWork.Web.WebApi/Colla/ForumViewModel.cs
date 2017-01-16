using System;
using System.Linq;
using System.Net.Http;
using AppBoot.Common;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    //讨论区
    public class ForumViewModel
    {
        public Guid TopicId { get; set; } 

        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public StaffViewModel Staff { get; set; }

        public ForumPostTypes TopicType { get; set; }

        public int CommentTotal { get; set; }
    
        //共识的状态， 0待共识，1，达成共识，-1未达成共识
        public int IsApproved { get; set; }

        public bool IsExpired { get; set; }

        public  DateTime? LastUpdatedAt { get; set; }

        public long ViewTotal { get; set; }

        public virtual void AssignFrom(ForumTopicEntity source,
         bool isShowhighOnly = false, bool isShowLow = true)
        {
            this.TopicId = source.Id;
            this.Title =string.IsNullOrEmpty(source.Title)?source.Content.Substring(0, source.Content.Length>20?20:source.Content.Length-1) : source.Title;
            this.TopicType = source.Type;
            this.Staff = source.Staff.ToViewModel(isShowhighOnly, isShowLow);
            this.CommentTotal = source.ForumComments.Count;
            this.CreatedAt = source.CreatedAt;
            this.LastUpdatedAt = source.LastUpdatedAt;
            this.ViewTotal = source.ViewTotal;
            if (TopicType == ForumPostTypes.Vote)
            {
                var vote = source.ForumVote.Vote;
                if (vote.IsApproved != null)
                    IsApproved = vote.IsApproved.Value ? 1 : -1;
                else 
                    IsApproved = 0;
                IsExpired = vote.EndAt >DateTime.Now;
                this.Title = vote.Subject;
            }
        }
    } 

    //管理维度
    public class ForumSectionViewModel
    {
        public Guid ForumSectionId { get; set; }

        public string Content { get; set; }

        public StaffViewModel Staff { get; set; }

        public ForumSections Section { get; set; }

        public DateTime CreatedAt { get; set; } 
        public virtual void AssignFrom(ForumSectionEntity source,
            bool isShowhighOnly = false, bool isShowLow = true)
        {
            this.ForumSectionId = source.Id;
            this.Content = source.Content;
            this.Staff = source.Staff.ToViewModel(isShowhighOnly, isShowLow);
            this.Section = source.Section;
            this.CreatedAt = source.CreatedAt;
        }
    }

    public class SimpleTopicViewModel
    {
        public Guid TopicId { get; set; }

        public ForumPostTypes TopicType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public StaffViewModel Staff { get; set; }

        public DateTime CreatedAt { get; set; }

        public ForumSections Section { get; set; }
        public string Version { get; set; }

        public virtual void AssignFrom(ForumTopicEntity source, Guid accountId,
            bool isShowhighOnly = false, bool isShowLow = true)
        {
            Args.NotNull(source, nameof(source));

            this.TopicId = source.Id;
            this.Staff = source.Staff.ToViewModel(isShowhighOnly, isShowLow);
            this.Content = source.Content;
            this.Title = String.IsNullOrEmpty( source.Title)?Content.Substring(0,Content.Length>20?20:Content.Length):source.Title; 
            this.TopicType = source.Type;
            this.CreatedAt = source.CreatedAt;
            this.Section = source.ForumSection.Section;
            if (source.Type == ForumPostTypes.Vote)
            {
                var vote = source.ForumVote.Vote;
                this.Content = vote.Subject;
            }
        }


    }

        //主题
    public class ForumTopicViewModel : SimpleTopicViewModel
    {
        public VoteViewModel Vote { get; set; }

        public long ViewTotal { get; set; }

        public long LikeTotal { get; set; }

        public long CommentTotal { get; set; }

        public bool LikeFlag { get; set; }

        public int IsApproved { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public override void AssignFrom(ForumTopicEntity source, Guid accountId,
            bool isShowhighOnly = false, bool isShowLow = true)
        {
            Args.NotNull(source, nameof(source));

            base.AssignFrom(source, accountId, isShowhighOnly, isShowLow);
            this.ViewTotal = source.ViewTotal;
            this.CommentTotal = source.ForumComments.Count;
            this.LikeTotal = source.ForumLikes.Count;

            this.LastUpdatedAt = source.LastUpdatedAt;
            if (source.Type == ForumPostTypes.Vote)
            {
                var vote = source.ForumVote.Vote;
                this.Vote = vote.ToViewModel(accountId);
                this.Content = vote.Subject;

                if (source.ForumVote.Vote.IsApproved != null)
                    IsApproved = source.ForumVote.Vote.IsApproved.Value ? 1 : -1;
                else
                    IsApproved = 0;
            }
        }
    }

    public class UnReadForumViewModel
    {
        public  string Version { get; set; }
        public ForumSections Section { get; set; } 

        public Guid ForumId { get; set; }

        public Guid? CommentId { get; set; }

        public string Content { get; set; }

        public Guid? LikeId { get; set; }

        public Guid AccountId { get; set; }

        public string StaffName { get; set; }

        public string SecurityStamp { get; set; }

        public bool IsLike { get; set; }

        public Guid TopicId { get; set; }

        public string TopicCotent { get; set; }

        public Guid? TargetCommentId { get; set; }

        public string TargetCommentContent { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class ForumCommentViewModel
    {
        public Guid CommentId { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? TargetCommentId { get; set; }

        public string TargetContent { get; set; }

        public DateTime? LastUpdatedAt { get; set; } 

        public StaffViewModel Staff { get; set; }  

        public long LikeTotal { get; set; }

        public bool LikeFlag { get; set; }

        public virtual void AssignFrom(ForumCommentEntity source,Guid accountId,
            bool isShowhighOnly = false, bool isShowLow = true)
        {
            this.CommentId = source.Id;
            this.Comment = source.Content;
            this.CreatedAt = source.CreatedAt;
            this.TargetCommentId = source.TargetComment?.Id;
            this.TargetContent = source.TargetContent;
            this.Staff = source.Staff.ToViewModel(isShowhighOnly, isShowLow);
            this.LikeTotal = source.Likes.Count;
            this.LikeFlag = source.Likes.Any(p => p.Staff.Account.Id == accountId);
            this.LastUpdatedAt = source.LastUpdatedAt;
        }
    }

    public static class ForumSectionViewModelExtensions
    {
        public static ForumSectionViewModel ToViewModel(this ForumSectionEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ForumSectionViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    } 

    public static class ForumViewModelExtensions
    {
        public static ForumViewModel ToForumViewModel(this ForumTopicEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ForumViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }

    public static class ForumTopicViewModelExtensions
    {
        public static ForumTopicViewModel ToViewModel(this ForumTopicEntity entity,Guid accountId, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ForumTopicViewModel();
            result.AssignFrom(entity,accountId, isShowhighOnly, isShowLow);
            return result;
        }

        public static SimpleTopicViewModel ToSimpleViewModel(this ForumTopicEntity entity, Guid accountId, bool isShowhighOnly = false,
          bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new SimpleTopicViewModel();
            result.AssignFrom(entity, accountId, isShowhighOnly, isShowLow);
            return result;
        }

    }

    public static class ForumCommentViewModelExtensions
    {
        public static ForumCommentViewModel ToViewModel(this ForumCommentEntity entity, Guid accountId,bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ForumCommentViewModel();
            result.AssignFrom(entity, accountId,isShowhighOnly, isShowLow);
            return result;
        }
    } 
}