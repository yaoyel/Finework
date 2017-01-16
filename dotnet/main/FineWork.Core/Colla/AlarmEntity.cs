using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AlarmEntity : EntityBase<Guid>
    {
        public bool IsEnabled { get; set; } = true;

        public int Weekdays { get; set; }

        public string ShortTime { get; set; }

        public string Bell { get; set; }

        public virtual TaskEntity Task { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string ReceiverKinds { get; set; }

        public string ReceiverStaffIds { get; set; }

        public bool IsRepeat { get; set; }

        public string Content { get; set; }

        public DateTime? NoRepeatTime { get; set; }

        public string DaysInMonth { get; set; }

        public AlarmTempKinds TempletKind { get; set; }

        public int? AttSize { get; set; }
    }

    [NotMapped]
    public class AlarmTempEntity : AlarmEntity
    {
    }
}