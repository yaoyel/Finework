using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncExecutorEntity:EntityBase<Guid>
    {  
        public virtual  StaffEntity Staff { get; set; }

        public virtual  AnnouncementEntity Annc { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}