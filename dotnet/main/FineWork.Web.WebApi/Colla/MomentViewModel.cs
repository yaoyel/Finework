using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{
    public class MomentViewModel
    {
        public Guid MomentId { get; set; }

        public string Content { get; set; }

        public MomentType Type { get; set; }

        public StaffViewModel Staff { get; set; }
         
        public List<MomentLikeViewModel>  Likes { get; set; }

        //若共享类型为附件是，显示的附件名称
        public string AttachmentName { get; set; }

        //当共享类型为非文字时 上传的图片或附件的id
        public Guid[] MomentFileIds { get; set; }

        public List<MomentCommentViewModel> Comments { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void  AssignFrom(MomentEntity moment)
        {
            this.MomentId = moment.Id;
            this.Content = moment.Content;
            this.Type = moment.Type;
            if (moment.Type == MomentType.Attachment && moment.MomentFiles.Any())
                this.AttachmentName = moment.MomentFiles.First().Name;
            if (moment.Type != MomentType.Word)
                this.MomentFileIds = moment.MomentFiles.OrderBy(p=>p.CreatedAt).Select(p => p.Id).ToArray();

            this.Staff = moment.Staff.ToViewModel(true);
            this.CreatedAt = moment.CreatedAt;
            this.Likes = moment.MomentLikes.Select(p => p.ToViewModel()).ToList();
            this.Comments = moment.MomentComments.Select(p => p.ToViewModel(true)).ToList();
        }
    }

    public class MomentLikeViewModel
    {
        public Guid LikeId { get; set; }

        public StaffViewModel Staff { get; set; } 

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(MomentLikeEntity momentLike)
        {
            this.LikeId = momentLike.Id; 
            this.Staff = momentLike.Staff.ToViewModel(true);
            this.CreatedAt = momentLike.CreatedAt; 
        }
    }

    public class MomentCommentViewModel
    {
        public Guid MomentId { get; set; }

        public Guid[] MomentFileIds { get; set; }

        public string AttachmentName { get; set; }

        public string Content { get; set; }

        public MomentType Type { get; set; }

        [Necessity]
        public Guid CommentId { get; set; }

        [Necessity]
        public Guid? TargetCommentId { get; set; }

        [Necessity]
        public string Comment { get; set; }

        [Necessity]
        public DateTime CreatedAt { get; set; }

        [Necessity]
        public StaffViewModel Staff { get; set; }

        public bool IsLike { get; set; }

        public virtual void AssignFrom(MomentCommentEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var propertiesDic = new Dictionary<string, Func<MomentCommentEntity, dynamic>>
            {
                ["MomentId"] = (t) => t.Moment.Id,
                ["Type"] = (t) => t.Moment.Type,
                ["Content"] = (t) => t.Moment.Content,
                ["AttachmentName"] = (t) =>
                {
                    if (t.Moment.Type == MomentType.Attachment && t.Moment.MomentFiles.Any())
                        return t.Moment.MomentFiles.First().Name;

                    return string.Empty;
                },

                ["MomentFileIds"] = (t) =>
                {
                    if (t.Moment.Type != MomentType.Word)
                        return t.Moment.MomentFiles.OrderBy(p => p.CreatedAt).Select(p => p.Id).ToArray();
                    else
                        return null;
                },
                ["CommentId"] = (t) => t.Id,
                ["Staff"] = (t) => t.Staff.ToViewModel(true),
                ["CreatedAt"] = (t) => t.CreatedAt,
                ["Comment"] = (t) => t.Comment,
                ["TargetCommentId"] = (t) => t.TargetComment?.Id,
                ["IsLike"]=(t)=>false
            };

            NecessityAttributeUitl<MomentCommentViewModel, MomentCommentEntity>.SetVuleByNecssityAttribute(this, source,
                propertiesDic, isShowhighOnly,
                isShowLow);
        }

        public virtual void AssignFrom(MomentLikeEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var propertiesDic = new Dictionary<string, Func<MomentLikeEntity, dynamic>>
            {
                ["MomentId"] = (t) => t.Moment.Id,
                ["Type"] = (t) => t.Moment.Type,
                ["Content"] = (t) => t.Moment.Content,
                ["AttachmentName"] = (t) =>
                {
                    if (t.Moment.Type == MomentType.Attachment && t.Moment.MomentFiles.Any())
                        return t.Moment.MomentFiles.First().Name;

                    return string.Empty;
                },

                ["MomentFileIds"] = (t) =>
                {
                    if (t.Moment.Type != MomentType.Word)
                        return t.Moment.MomentFiles.OrderBy(p => p.CreatedAt).Select(p => p.Id).ToArray();
                    else
                        return null;
                },
                ["CommentId"] = (t) => t.Id,
                ["Staff"] = (t) => t.Staff.ToViewModel(true),
                ["CreatedAt"] = (t) => t.CreatedAt,
                ["Comment"] = (t) =>null,
                ["TargetCommentId"] = (t) => null,
                ["IsLike"] = (t) => true
            };

            NecessityAttributeUitl<MomentCommentViewModel, MomentLikeEntity>.SetVuleByNecssityAttribute(this, source,
                propertiesDic, isShowhighOnly,
                isShowLow);
        }
    } 

    public static class MomentViewModelExtensions
    {
        public static MomentViewModel ToViewModel(this MomentEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new MomentViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }

    public static class MomentLikeViewModelExtensions
    {
        public static MomentLikeViewModel ToViewModel(this MomentLikeEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new MomentLikeViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }

    public static class MomentCommentViewModelExtensions
    {
        public static MomentCommentViewModel ToViewModel(this MomentCommentEntity entity,bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new MomentCommentViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static MomentCommentViewModel ToCommentViewModel(this MomentLikeEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new MomentCommentViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
