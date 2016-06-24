using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskReportEntity:EntityBase<Guid>
    {
        public DateTime FinishedAt { get; set; }

        public string Summary { get; set; }

        public float EffScore { get; set; }

        public float QualityScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual TaskEntity Task { get; set; }

        public virtual ICollection<TaskExilsEntity> TaskExilses { get; set; } = new HashSet<TaskExilsEntity>();
    }
}
