using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncAttEntity:EntityBase<Guid>
    {
        public string Name { get; set; }

        public float Size { get; set; }

        public string ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //标记是成果还是共享
        public bool IsAchv { get; set; }

        [Timestamp]
        public  byte[] RowVer { get; set; }

        public virtual AnnouncementEntity Announcement { get; set; } 
    }
}
