using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using FineWork.Security.Repos.Aef;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FineWork.Colla
{
   public class StaffInvEntity:EntityBase<Guid>
    {
        public virtual OrgEntity Org { get; set; }
          
        public string PhoneNumber { get; set; }

        public string Message { get; set; }

        public string StaffName { get; set; }

        public string InviterNames { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReviewStatuses ReviewStatus { get; set; } 

        public DateTime? ReviewAt { get; set; }  
    }
}
