using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class MomentCommentEntity:EntityBase<Guid>
    {
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; 

        public virtual MomentCommentEntity TargetComment { get; set; }

        public virtual MomentCommentEntity DerivativeComment { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; } 

        public virtual StaffEntity Staff { get; set; }

        public virtual MomentEntity Moment { get; set; }

       
    }
}
