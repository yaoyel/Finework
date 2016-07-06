using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumComment:EntityBase<Guid>
    {
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;


        public ForumComment TargetComment { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual StaffEntity Staff { get; set; }
    }
}