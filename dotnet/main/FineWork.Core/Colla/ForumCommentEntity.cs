using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumCommentEntity:EntityBase<Guid>
    {
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now; 

        public DateTime? LastUpdatedAt { get; set; }

        public virtual ForumCommentEntity TargetComment { get; set; }

        public string TargetContent { get; set; }

        public virtual  ICollection<ForumCommentEntity> DerivativeComments { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public virtual ForumTopicEntity ForumTopic { get; set; }

        public virtual ICollection<ForumCommentLikeEntity> Likes { get; set; }=new HashSet<ForumCommentLikeEntity>(); 
    }
}