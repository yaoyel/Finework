using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.UI.WebControls;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncAlarmEntity:EntityBase<Guid>
    {
        public virtual AnnouncementEntity Annc { get; set; }

        public  DateTime? Time { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public string Bell { get; set; }

        public int? BeforeStart { get; set; }

        public bool IsEnabled { get; set; } = true;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual ICollection<AnncAlarmRecEntity> Recs { get; set; }=new HashSet<AnncAlarmRecEntity>();
    }


    public class AnncAlarmRecEntity : EntityBase<Guid>
    {
        public virtual AnncAlarmEntity AnncAlarm { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public AnncRoles AnncRole { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}