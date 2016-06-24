using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Security.Crypto;

namespace FineWork.Colla
{
    public class InvCodeEntity
    {
        public string Id { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiredAt { get; set; }

        public virtual OrgEntity Org { get; set; } = null;
    }
}
