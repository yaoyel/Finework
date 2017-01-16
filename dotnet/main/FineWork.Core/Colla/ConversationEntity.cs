using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ConversationEntity:EntityBase<string>
    {
        public virtual ICollection<MemberEntity> Members { get; set; }=new HashSet<MemberEntity>();

        public virtual ICollection<TaskAlarmEntity> TaskAlarms { get; set; }=new HashSet<TaskAlarmEntity>();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public  byte[] RowVer { get; set; }

        public bool? IsEnabled { get; set; }

        public bool? IsUnique { get; set; }
    }
}