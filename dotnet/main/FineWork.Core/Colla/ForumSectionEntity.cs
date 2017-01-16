using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumSectionEntity:EntityBase<Guid>
    {
        public ForumSections Section { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }


        public virtual StaffEntity Staff { get; set; }

        public virtual ICollection<ForumTopicEntity> ForumTopics { get; set; }=new HashSet<ForumTopicEntity>();

        public virtual ICollection<ForumSectionViewEntity> ViewStaffs { get; set; }=new HashSet<ForumSectionViewEntity>();
    }
}