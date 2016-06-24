using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskAnnouncementEntity : EntityBase<Guid>
    { 
        public AnnouncementKinds AnnounceKind { get; set; }

        public bool IsGoodNews { get; set; } = false;

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual TaskEntity Task { get; set; }

        public virtual StaffEntity Staff { get; set; }
    }
}
