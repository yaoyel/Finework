using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineWork.Core.Colla.Models
{
    public class CreateStaffInvModel
    {   
        public Guid StaffId { get; set; }

        public Guid OrgId { get; set; }

        public string Message { get; set; }

        public string InviterName { get; set; }

        //item1 name,item2 phonenumber
        public IList<Tuple<string,string>> Invitees { get; set;}
         
    }
}
