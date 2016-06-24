using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskExilsEntity:EntityBase<Guid>
    {
        public Guid PartakerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TaskReportEntity TaskReport { get; set; }
    }
}
