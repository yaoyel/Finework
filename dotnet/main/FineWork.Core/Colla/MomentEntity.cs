using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class MomentEntity : EntityBase<Guid>
    {
         public MomentType Type { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public virtual ICollection<MomentFileEntity> MomentFiles { get; set; } = new HashSet<MomentFileEntity>();

        public virtual ICollection<MomentLikeEntity> MomentLikes { get; set; } = new HashSet<MomentLikeEntity>();
        public virtual ICollection<MomentCommentEntity> MomentComments { get; set; } =
            new HashSet<MomentCommentEntity>();
    }
}
