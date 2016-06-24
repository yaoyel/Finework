using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public  class MomentLikeEntity:EntityBase<Guid>
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual MomentEntity Moment { get; set; }

        public virtual StaffEntity Staff { get; set; }
    }
}
