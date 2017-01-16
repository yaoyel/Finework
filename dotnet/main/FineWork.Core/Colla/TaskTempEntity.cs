using System;
using System.Collections.Generic;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskTempEntity:EntityBase<Guid>
    {
        public virtual StaffEntity Staff { get; set; }

        public virtual TaskEntity Task { get; set; }

        public DateTime CreatedAt { get; set; }  =DateTime.Now;

        public DateTime? LastUpdatedAt { get; set; }

        //public int Copys { get; set; }

        //public int TempKinds { get; set; }
    } 
}