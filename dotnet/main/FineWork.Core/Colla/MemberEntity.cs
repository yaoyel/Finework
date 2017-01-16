using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class MemberEntity:EntityBase<Guid>
    {
        public virtual StaffEntity Staff { get; set; } 

        public virtual ConversationEntity Conversation { get; set; }

        [Timestamp]
        public byte[] RowVer { get; set; }

        public DateTime? ClearLogAt { get; set; }
    }
}
