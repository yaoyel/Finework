using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class PlanEntity:EntityBase<Guid>
    {
        public PlanType Type { get; set; }

        public String Content { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public int Stars { get; set; }

        public bool IsPrivate { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public string MonthOrYear { get; set; }

        public virtual StaffEntity Creator { get; set; }

        public virtual StaffEntity Inspecter { get; set; }

        public bool ExecFrPartaker { get; set; } = true;

        public virtual  ICollection<PlanAlarmEnitty> Alarms { get; set; }=new HashSet<PlanAlarmEnitty>();

        public virtual  ICollection<PlanExecutorEntity> Executors { get; set; }=new HashSet<PlanExecutorEntity>();

        public virtual ICollection<PlanAtEntity> Ats { get; set; }=new HashSet<PlanAtEntity>();
    }
}