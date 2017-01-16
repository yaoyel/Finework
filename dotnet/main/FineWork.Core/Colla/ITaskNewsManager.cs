using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskNewsManager
    {
        TaskNewsEntity CreateTaskNews(CreateTaskNewsModel taskAnnounceModel);

        IEnumerable<TaskNewsEntity> FetchTaskNewsesByTaskId(Guid taskId);

        IEnumerable<TaskNewsEntity> FetchTaskNewsesByStaffId(Guid staffId);

        TaskNewsEntity FindTaskNewsById(Guid taskNewsId); 

        void DeleteTaskNewsById(Guid taskNewsId);

        Tuple<int,IEnumerable<Guid>> FetchUnReadNewsesByStaffId(Guid staffId); 
    }
}