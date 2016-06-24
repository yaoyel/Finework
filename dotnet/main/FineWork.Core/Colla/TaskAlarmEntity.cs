using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{ 
    public class TaskAlarmEntity:EntityBase<Guid>
    {
        public TaskAlarmKinds TaskAlarmKind { get; set; }

        public string Content { get; set; }

        public ResolveStatus ResolveStatus { get; set; } = ResolveStatus.UnResolved;

        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ClosedAt { get; set; }

        public string Comment { get; set; }

        public string ConversationId { get; set; }

        public string AlarmDesc { get; set; }  

        [NotMapped]
        public   int[] ReceiversArray {
            get { return !string.IsNullOrEmpty(this.Receivers)?Array.ConvertAll(this.Receivers.Split(','), int.Parse):null; }
            set { this.Receivers = string.Join(",", value); } 
        }

        public string Receivers { get; set; }

        public virtual TaskEntity Task { get; set; }

        public virtual StaffEntity Staff { get; set; }

    }
}
