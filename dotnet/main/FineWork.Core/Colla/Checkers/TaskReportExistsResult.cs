using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class TaskReportExistsResult : FineWorkCheckResult
    {
        public TaskReportExistsResult(bool isSucceed, String message, TaskReportEntity taskReport)
            : base(isSucceed, message)
        {
            this.TaskReport = taskReport;
        }

        public TaskReportEntity TaskReport { get; set; }
        public static TaskReportExistsResult Check(ITaskReportManager taskReportManager, Guid reportId)
        {
            TaskReportEntity taskReporty = taskReportManager.FindTaskReportById(reportId);
            return Check(taskReporty, "任务报告不存在.");
        }

      
        private static TaskReportExistsResult Check([CanBeNull]  TaskReportEntity taskReport, String message)
        {
            if (taskReport == null)
            {
                return new TaskReportExistsResult(false, message, null);
            }

            return new TaskReportExistsResult(true, null, taskReport);
        }
    }
}
