using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;
using FineWork.Security.Repos.Aef;

namespace FineWork.Colla
{
    public class OrgEntity : EntityBase<Guid>
    {
        public String Name { get; set; }

        public bool IsInvEnabled { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public String SecurityStamp { get; set; } = Guid.NewGuid().ToString("D");

        [Timestamp]
        public virtual Byte[] RowVer { get; set; }

        /// <summary> 组织的管理员. </summary>
        public virtual StaffEntity AdminStaff { get; set; }

        public virtual ICollection<StaffEntity> Staffs { get; set; } = new HashSet<StaffEntity>();

        public virtual ICollection<StaffReqEntity> StaffReqs { get; set; } = new HashSet<StaffReqEntity>();
        public virtual ICollection<StaffInvEntity> StaffInvs { get; set; } = new HashSet<StaffInvEntity>();
    }
}
