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
    public class AnnouncementEntity:EntityBase<Guid>
    {
        public string Content { get; set; }
 
        public DateTime? EndAt { get; set; }
 
        public DateTime? StartAt { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public bool IsNeedAchv { get; set; }

        [NotMapped]
        public int Statrs { get; set; }

        [NotMapped]
        public PlanType Type { get; set; }

        [NotMapped]
        public bool IsPrivate { get; set; } = false;

        [NotMapped]
        public string MonthOrYear { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; } 

        public virtual TaskEntity Task { get; set; }

        public virtual StaffEntity Creator { get; set; } 

        public  virtual  StaffEntity Inspecter { get; set; }  

        public virtual ICollection<AnncAttEntity> Atts { get; set; } = new HashSet<AnncAttEntity>();

        public virtual ICollection<AnncReviewEntity> Reviews { get; set; }=new HashSet<AnncReviewEntity>(); 

        public virtual ICollection<AnncAlarmEntity> Alarms { get; set; }=new HashSet<AnncAlarmEntity>();

        public virtual ICollection<AnncExecutorEntity> Executors { get; set; }=new HashSet<AnncExecutorEntity>();

        public virtual ICollection<AnncUpdateEntity> Updates { get; set; }=new HashSet<AnncUpdateEntity>();
    }
}
