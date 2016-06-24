using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskReportModel
    {
        public Guid TaskId { get; set; }

        public DateTime FinishedAt { get; set; }

        public string Summary { get; set; }

        public float EffScore { get; set; }

        public float QualityScore { get; set; }

        public Guid[] Exilses { get; set; }
    }
}
