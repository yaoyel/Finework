using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Net.Push
{
    public class DeviceRegistration
    { 
        public string Platform { get; set; }
        public string PlatformDescription { get; set; }
        public string RegistrationId { get; set; }
        public Guid AccountId { get; set; }
    }
}
