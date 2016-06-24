using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskReportManager
    {
        TaskReportEntity CreateTaskReport(CreateTaskReportModel createTaskReportModel);

        TaskReportEntity UpdateTaskReport(UpdateTaskReportModel updateTaskReportModel);

        TaskReportEntity FindTaskReportById(Guid reportId);

        TaskReportEntity FindTaskReportByTaskId(Guid taskId);
    }
}
