using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla
{
    public class TaskIncentiveEntity : EntityBase<Guid>
    {
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual IncentiveKindEntity IncentiveKind { get; set; }

        public virtual TaskEntity Task { get; set; } = new TaskEntity();

        public virtual ICollection<IncentiveEntity> Incentives { get; set; } = new HashSet<IncentiveEntity>();
    }

}
