using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public  class PushLogEntity:EntityBase<Guid>
    {
        public Guid TargetId { get; set; }

        public PushKinds TargetType { get; set; }

        [NotMapped]
        public   TaskEntity Task { get; set; }

        [NotMapped]
        public string Content { get; set; }

        [NotMapped]
        public string ShortTime { get; set; }

        [NotMapped]
        public int Repeat { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public bool IsViewed { get; set; } = false;

        public DateTime CreatedAt { get; set; }

    }
}
