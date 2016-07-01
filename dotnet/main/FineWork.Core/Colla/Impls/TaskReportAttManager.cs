using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Security.Crypto;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Core;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class TaskReportAttManager : AefEntityManager<TaskReportAttEntity, Guid>, ITaskReportAttManager
    {
        public TaskReportAttManager(ISessionProvider<AefSession> sessionProvider,
             ILazyResolver<ITaskReportManager> taskReportManagerResolver,
            ITaskSharingManager taskSharingManager) : base(sessionProvider)
        {
            Args.NotNull(taskReportManagerResolver, nameof(taskReportManagerResolver));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));

            m_TaskReportManagerResolver = taskReportManagerResolver;
            m_TaskSharingManager = taskSharingManager;
        }

        private readonly ILazyResolver<ITaskReportManager> m_TaskReportManagerResolver;
        private readonly ITaskSharingManager m_TaskSharingManager;

        private ITaskReportManager TaskReportManager
        {
            get { return m_TaskReportManagerResolver.Required; }
        }

        public TaskReportAttEntity CreateReportAtt(Guid taskReportId, Guid taskSharingId)
        {

            var taskReport =
                TaskReportExistsResult.Check(this.TaskReportManager, taskReportId).ThrowIfFailed().TaskReport;
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

        public void UpdateTaskReportAtts(TaskReportEntity taskReport, Guid[] taskSharingIds)
        {
            Args.NotNull(taskReport, nameof(taskReport));
            var atts = taskReport.Atts.Select(p => p.TaskSharing.Id).ToArray();

            var newAtts = taskSharingIds.Except(atts).ToArray();

            if (newAtts.Any())
            {
                foreach (var att in newAtts)
                {
                    this.CreateReportAtt(taskReport.Id, att);
                }
            }

            var diffAtts = atts.Except(taskSharingIds).ToArray();

            if (diffAtts.Any())
            {
                foreach (var att in diffAtts)
                {
                    var attEntity =
                        this.InternalFetch(p => p.TaskReport.Id == taskReport.Id && p.TaskSharing.Id == att)
                            .FirstOrDefault();
                    if (attEntity != null)
                        this.InternalDelete(attEntity);
                }
            } 
        }

    }
}