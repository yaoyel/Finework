using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumTopicEntity:EntityBase<Guid>
    { 
        public ForumPostTypes Type { get; set; }

        public long ViewTotal { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public  ForumSectionEntity ForumSection { get; set; }=new ForumSectionEntity();


    }
}