using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskLogEntity: EntityBase<Guid>
    {
        public string TargetKind { get; set; }

        public Guid TargetId { get; set; }

        public string ActionKind { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TaskEntity Task { get; set; }

        public virtual StaffEntity Staff { get; set; }
    }
}
