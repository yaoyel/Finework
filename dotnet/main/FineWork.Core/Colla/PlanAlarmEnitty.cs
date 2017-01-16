using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class PlanAlarmEnitty:EntityBase<Guid>
    {
        public DateTime? Time { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public string Bell { get; set; }

        public bool IsEnabled { get; set; }

        //相对于开始时间，提前多少分钟进行提示
        public int? BeforeStart { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual  PlanEntity Plan { get; set; } 
    }
}