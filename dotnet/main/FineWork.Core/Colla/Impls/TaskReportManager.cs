using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;

namespace FineWork.Colla.Impls
{
    public class TaskReportManager :AefEntityManager<TaskReportEntity, Guid>, ITaskReportManager
    {

        public TaskReportManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager) : base(sessionProvider)
        {

            Args.NotNull(taskManager, nameof(taskManager));
            m_TaskManager = taskManager;
        }

        private readonly ITaskManager m_TaskManager;

        public TaskReportEntity CreateTaskReport(CreateTaskReportModel createTaskReportModel)
        {
            Args.NotNull(createTaskReportModel, nameof(createTaskReportModel));
            var task = TaskExistsResult.Check(this.m_TaskManager, createTaskReportModel.TaskId).ThrowIfFailed().Task;
            var taskReport = new TaskReportEntity();

            taskReport.Id = Guid.NewGuid();
            taskReport.FinishedAt = createTaskReportModel.FinishedAt;
            taskReport.Summary = createTaskReportModel.Summary;
            taskReport.EffScore = createTaskReportModel.EffScore;
            taskReport.QualityScore = createTaskReportModel.QualityScore;
            taskReport.Task = task;
            return taskReport;
        }

        public TaskReportEntity FindTaskReportById(Guid reportId)
        {
            return this.InternalFind(reportId);
        }

        public TaskReportEntity FindTaskReportByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId).FirstOrDefault();
        }

        public TaskReportEntity UpdateTaskReport(UpdateTaskReportModel updateTaskReportModel)
        {
            var report = TaskReportExistsResult.Check(this, updateTaskReportModel.Id).ThrowIfFailed().TaskReport;

            report.FinishedAt = updateTaskReportModel.FinishedAt;
            report.Summary = updateTaskReportModel.Summary;
            report.EffScore = updateTaskReportModel.EffScore;
            report.QualityScore = updateTaskReportModel.QualityScore;

            if (report.TaskExilses.Select(p => p.PartakerId).ToArray() != updateTaskReportModel.Exilses)
            {
                
            }

            this.InternalUpdate(report);
            return report;
        }

  
    }
}
