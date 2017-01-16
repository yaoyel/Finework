using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskReportAttEntity:EntityBase<Guid>
    { 
        public DateTime CreatedAt { get; set; } = DateTime.Now;  

        public virtual  TaskSharingEntity TaskSharing { get; set; }
          
        public virtual TaskReportEntity TaskReport { get; set; }
         
    }
}