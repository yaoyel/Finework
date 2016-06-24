using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace FineWork.Net.Push
{
    public interface IPushService
    { 
        Task<PushResult> SendAsync(string title,string message, IDictionary<string, string> customizedValue, params string[] registrationIds);  
    }
}
