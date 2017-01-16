using System;
using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla.Models
{
    public class AlarmPeriodModel
    {
        [MaxLength(400,ErrorMessage = "预警内容不能超过400个字.")]
        public  string Content { get; set; }

        public Guid? TaskId { get; set; }

        public int? Weekdays { get; set; }

        public string ShortTime { get; set; }

        public string Bell { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool IsRepeat { get; set; } = true;

        public  string DaysInMonth { get; set; }

        public DateTime? NoRepeatTime { get; set; }

        public AlarmTempKinds TempletKind { get; set; }

        public  int? AttSize { get; set; }
    }

    public class CreateAlarmPeriodModel:AlarmPeriodModel
    {
        public PartakerKinds[] ReceiverKinds { get; set; }

        public Guid[] ReceiverStaffIds { get; set; }
    }

    public class UpdateAlarmPeriodModel : CreateAlarmPeriodModel
    {
        public  Guid AlarmId { get; set; }
    }
}