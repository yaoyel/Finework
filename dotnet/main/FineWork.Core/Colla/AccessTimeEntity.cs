using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AccessTimeEntity: EntityBase<Guid>
    {
        public DateTime? LastEnterOrgAt { get; set; }

        public DateTime? LastViewMomentAt { get; set; }

        public DateTime? LastViewCommentAt { get; set; }

        public DateTime? LastViewNewsAt { get; set; }

        public Guid StaffId { get; set; }

        public StaffEntity Staff { get; set; }
    }
}
