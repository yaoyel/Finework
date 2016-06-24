using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreatePartakerInvModel
    {
        public Guid TaskId { get; set; }

        public Guid[] InviteeStaffIds { get; set; }

        public PartakerKinds PartakerKind { get; set; }
    }
}
