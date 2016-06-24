using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IMomentFileManager
    {
        void CreateMementFile(Guid mementId, string contentType, Stream fileStream,string fileName="", int momentFileCount = 1);

        void DeleteMementFileByMementId(Guid mementId);

        void UploadMomentBgImage(OrgEntity orgId,string contentType,Stream fileStream);

        void DownloadMomentFile(MomentFileEntity momentFile, Stream stream);

        MomentFileEntity FindMomentFileById(Guid momentFileId);
    }
}
