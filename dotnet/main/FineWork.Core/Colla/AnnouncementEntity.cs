using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnnouncementEntity:EntityBase<Guid>
    {
        public string Content { get; set; }

        public DateTime EndAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsNeedAchv { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public virtual TaskEntity Task { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public virtual ICollection<AnncAttEntity> Atts { get; set; } = new HashSet<AnncAttEntity>();

        public virtual ICollection<AnncIncentiveEntity> AnncIncentives { get; set; } =
            new HashSet<AnncIncentiveEntity>();
    }
}
