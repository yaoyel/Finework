using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using FineWork.Security;
using FineWork.Security.Repos.Aef;

namespace FineWork.Message
{
    public class DeviceRegistrationEntity : EntityBase<Guid>
    {   
        public string Platform { get; set; }
        public string PlatformDescription { get; set; }  
        public string RegistrationId { get; set; }   

        public DateTime CreatedAt { get; set; }

        [Timestamp]
        public virtual Byte[] RowVer { get; set; }

        public virtual AccountEntity Account { get; set; }
    } 
}
