using System;
using System.Collections.Generic;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskSnapshotEntity : EntityBase<Guid>
    {
        public string Snapshot { get; set; } 

        public TaskSnapshotEntity ParentTaskSnapshot { get; set; }

        public  DateTime CreatedAt { get; set; }=DateTime.Now;

        public string Anncs { get; set; }

        public string Notes { get; set; }

        public string Votes { get; set; }
 
        public virtual  TaskEntity Task { get; set; }

     }
}