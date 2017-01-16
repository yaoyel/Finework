using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface ITaskSharingManager
    {
        TaskSharingEntity CreateTaskSharing(Guid taskId, Guid staffId, string fileName, string contentType,Stream fileStream);

        void DownloadTaskSharing(TaskSharingEntity taskSharing, Stream stream);

        TaskSharingEntity CreateTaskSharing(Guid taskId, Guid staffId, Guid taskSharingId);
        IEnumerable<TaskSharingEntity> FetchTaskSharingsByTask(Guid taskId);

        void DeleteTaskSharing(Guid taskSharingId);

        TaskSharingEntity FindTaskSharing(Guid taskSharingId);

       TaskSharingEntity FindTaskSharingWithTask(Guid taskId, Guid taskSharingId);

        IEnumerable<TaskSharingEntity> FetchTaskSharingByContentMd5(string contentMd5);
    }
}
