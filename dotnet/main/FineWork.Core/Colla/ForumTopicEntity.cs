using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumTopicEntity:EntityBase<Guid>
    { 
        public ForumPostTypes Type { get; set; }

        public long ViewTotal { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;
         
        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual ForumSectionEntity ForumSection { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public  virtual ICollection<ForumCommentEntity> ForumComments { get; set; }=new HashSet<ForumCommentEntity>();

        public virtual ICollection<ForumLikeEntity> ForumLikes { get; set; }=new HashSet<ForumLikeEntity>();

        public virtual  ForumVoteEntity  ForumVote{ get; set; }

        public DateTime? LastUpdatedAt { get; set; }
    }
}