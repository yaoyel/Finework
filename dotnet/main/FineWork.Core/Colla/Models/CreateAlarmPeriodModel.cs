using System;

namespace FineWork.Colla.Models
{
    public class AlarmPeriodModel
    { 
            public Guid? TaskId { get; set; }

            public int Weekdays { get; set; }

            public string ShortTime { get; set; }

            public string Bell { get; set; }

            public bool IsEnabled { get; set; } = true; 
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