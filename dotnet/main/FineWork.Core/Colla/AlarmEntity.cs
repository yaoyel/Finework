using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AlarmEntity:EntityBase<Guid>
    {
        public bool IsEnabled { get; set; } = true;

        public int Weekdays { get; set; }

        public string ShortTime { get; set; }

        public string Bell { get; set; }

        public virtual TaskEntity Task { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
         
    }
}
