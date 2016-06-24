using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Repos
{
    /// <summary> Represents the callbacks for repository events. </summary>
    public interface IRepositoryCallback
    {
        void HandleRepositoryChanged(IRepository repository);

        Task HandleRepositoryChangedAsync(IRepository repository);
    }
}
