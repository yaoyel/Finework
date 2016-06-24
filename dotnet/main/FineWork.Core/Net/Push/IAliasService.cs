using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Net.Push
{
    public interface IAliasService
    {
        Task UpdateAliasAsync(string registrationId,string items);

        Task<PushResult> SendByAliasAsync(string title,string message, IDictionary<string, string> customizedValue, params string[] aliases);

    }
}
