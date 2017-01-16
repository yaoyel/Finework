using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class PlanExecutorEntity:EntityBase<Guid>
    {
        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public virtual  StaffEntity Staff { get; set; }

        public virtual  PlanEntity Plan { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

    }
}