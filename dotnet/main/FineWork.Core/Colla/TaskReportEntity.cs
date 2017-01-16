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
        public DateTime EndedAt { get; set; }

        public string Summary { get; set; }

        public decimal EffScore { get; set; }

        public decimal QualityScore { get; set; }

        public DateTime CreatedAt { get; set; }  

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual TaskEntity Task { get; set; } 

        public virtual ICollection<TaskReportAttEntity> Atts { get; set; } = new HashSet<TaskReportAttEntity>();
    }
}
