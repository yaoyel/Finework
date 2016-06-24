using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class IncentiveKindEntity:EntityBase<int>
    {
        public string Name { get; set; }

        public string Unit { get; set; }

        public virtual ICollection<TaskIncentiveEntity> TaskIncentives { get; set; } = new HashSet<TaskIncentiveEntity>();

        public virtual ICollection<AnncIncentiveEntity> AnncIncentives { get; set; } =
            new HashSet<AnncIncentiveEntity>();
    }
}
