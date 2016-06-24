using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Net.Push
{
    public interface ITagsService
    {
        Task UpdateTagsAsync(string registrationId,HashSet<string> tagsToAdd,HashSet<string> tagsToRemove);

        Task AddTagsAsync(string registrationId,params string[] items );
         

        Task DeleteTagsAsync(string registrationId,params string[] items);

        Task<PushResult> SendByTagsAsync(string title, string message, IDictionary<string, string> customizedValue, params string[] tags);
    }
}
