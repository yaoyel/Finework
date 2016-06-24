using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineWork.Web.WebApp.Models
{
    public class AccountViewModel
    {
        public  Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; } 

    }
}
