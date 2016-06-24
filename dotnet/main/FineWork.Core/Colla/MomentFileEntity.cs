using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class MomentFileEntity: EntityBase<Guid>
    {
       public string Path { get; set; }

        public string Name { get; set; } 

        public string ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual MomentEntity Moment { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}
