using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class PlanAtEntity:EntityBase<Guid>
    {
        
        public DateTime CreatedAt { get; set; }=DateTime.Now;

        public byte[] RowVer { get; set; }

        public virtual  StaffEntity Staff { get; set; }

        public virtual PlanEntity Plan { get; set; }

        public bool IsChecked { get; set; } = false;
    }
}