using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskSharingEntity:EntityBase<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FileName { get; set; }

        public string Blocks { get; set; }

        public string ContentMd5 { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public virtual  StaffEntity Staff { get; set; }

        public virtual TaskEntity Task { get; set; } 
         
    }
}
