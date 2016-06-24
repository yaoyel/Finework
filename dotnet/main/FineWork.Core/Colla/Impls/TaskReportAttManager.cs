using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Security.Crypto;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class TaskReportAttManager : AefEntityManager<TaskReportAttEntity, Guid>, ITaskReportAttManager
    {
        public TaskReportAttManager(ISessionProvider<AefSession> sessionProvider,
            ITaskReportManager taskReportManager,
            ITaskSharingManager taskSharingManager) : base(sessionProvider)
        {
            Args.NotNull(taskReportManager, nameof(taskReportManager));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));

            m_TaskReportManager = taskReportManager;
            m_TaskSharingManager = taskSharingManager;
        }

        private readonly ITaskReportManager m_TaskReportManager;
        private readonly ITaskSharingManager m_TaskSharingManager;

        public TaskReportAttEntity CreateReportAtt(Guid taskReportId, Guid taskSharingId)
        {

            var taskReport =
                TaskReportExistsResult.Check(this.m_TaskReportManager, taskReportId).ThrowIfFailed().TaskReport;
            var taskSharing =
                TaskSharingExistsResult.Check(this.m_TaskSharingManager, taskSharingId).ThrowIfFailed().TaskSharing;

            var reportAtt = new TaskReportAttEntity();
            reportAtt.TaskReport = taskReport;
            reportAtt.Id = Guid.NewGuid();
            reportAtt.TaskSharing = taskSharing;

            this.InternalInsert(reportAtt);
            return reportAtt;

        }



        public IEnumerable<TaskReportAttEntity> FetchAttsByReortId(Guid taskReportId)
        {
            return this.InternalFetch(p => p.TaskReport.Id == taskReportId);
        }


        public void DeleteTaskReportAtt(Guid reportAttId)
        {
            var att = this.InternalFind(reportAttId);
            if (att == null) return;

            this.InternalDelete(att);
        }

    }
}