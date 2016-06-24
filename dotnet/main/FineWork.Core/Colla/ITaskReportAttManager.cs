using System;
using System.Collections.Generic;
using System.IO;

namespace FineWork.Colla
{
    public interface ITaskReportAttManager
    {
        TaskReportAttEntity CreateReportAtt(Guid taskReportId,Guid taskSharingId);

        IEnumerable<TaskReportAttEntity> FetchAttsByReortId(Guid taskReportId); 

        void DeleteTaskReportAtt(Guid reportAttId);
    }
}