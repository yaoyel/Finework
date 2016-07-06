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
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class TaskReportManager : AefEntityManager<TaskReportEntity, Guid>, ITaskReportManager
    { 
        public TaskReportManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IPartakerManager partakerManager,
            ITaskReportAttManager taskReportAttManager,
            ITaskSharingManager taskSharingManager,
            IIMService imService) : base(sessionProvider)
        {

            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(taskReportAttManager, nameof(taskReportAttManager));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));
            Args.NotNull(imService, nameof(imService));

            m_PartakerManager = partakerManager;
            m_TaskManager = taskManager;
            m_TaskReportAttManager = taskReportAttManager;
            m_TaskSharingManager = taskSharingManager;
            m_Imservice = imService;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly ITaskReportAttManager m_TaskReportAttManager;
        private readonly ITaskSharingManager m_TaskSharingManager;
        private readonly IIMService m_Imservice;

        public TaskReportEntity CreateTaskReport(CreateTaskReportModel createTaskReportModel)
        {
            Args.NotNull(createTaskReportModel, nameof(createTaskReportModel));
            var task = TaskExistsResult.Check(this.m_TaskManager, createTaskReportModel.TaskId).ThrowIfFailed().Task;
            var taskReport = new TaskReportEntity();

            taskReport.Id = Guid.NewGuid();
            taskReport.EndedAt = createTaskReportModel.EndedAt;
            taskReport.Summary = createTaskReportModel.Summary;
            taskReport.EffScore = createTaskReportModel.EffScore;
            taskReport.QualityScore = createTaskReportModel.QualityScore;
            taskReport.Task = task;

            this.InternalInsert(taskReport);
            //设置表现突出的战友
            if (createTaskReportModel.Exilses.Any())
            {
                foreach (var exils in createTaskReportModel.Exilses)
                {
                    var partaker = PartakerExistsResult.Check(task, exils).ThrowIfFailed().Partaker;
                    partaker.IsExils = true;
                    m_PartakerManager.UpdatePartaker(partaker);
                }
            }
             
            //添加附件
            if (createTaskReportModel.Atts!=null && createTaskReportModel.Atts.Any())
            {
                foreach (var attId in createTaskReportModel.Atts)
                {
                    var taskSharing = TaskSharingExistsResult.Check(m_TaskSharingManager,attId).ThrowIfFailed().TaskSharing;
                    m_TaskReportAttManager.CreateReportAtt(taskReport.Id, taskSharing.Id);
                }
            }

            Task.Factory.StartNew(() =>
            {
                m_Imservice.ChangeConAttr(task.Creator.Id.ToString(),task.ConversationId, "IsEnd", true);
            });
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

            report.EndedAt = updateTaskReportModel.EndedAt;
            report.Summary = updateTaskReportModel.Summary;
            report.EffScore = updateTaskReportModel.EffScore;
            report.QualityScore = updateTaskReportModel.QualityScore;


            var exilsIds = m_PartakerManager.FetchExilsesByTaskId(report.Task.Id).Select(p => p.Id).ToArray();

            
            if (exilsIds.Any() || updateTaskReportModel.Exilses.Any())
            {
                var diffExilses = exilsIds.Except(updateTaskReportModel.Exilses).ToArray();

                //取消的优秀战友
                if (diffExilses.Any())
                    foreach (var partakerId in diffExilses)
                    {
                        var partaker = PartakerExistsResult.Check(report.Task, partakerId).Partaker;
                        if (partaker != null)
                        {
                            partaker.IsExils = null;
                            m_PartakerManager.UpdatePartaker(partaker);
                        }
                    }

                //新增的优秀战友
                diffExilses = updateTaskReportModel.Exilses.Except(exilsIds).ToArray();
                if (diffExilses.Any())
                    foreach (var partakerId in diffExilses)
                    {
                        var partaker = PartakerExistsResult.Check(report.Task, partakerId).ThrowIfFailed().Partaker;

                        partaker.IsExils = true;
                        m_PartakerManager.UpdatePartaker(partaker);

                    } 
            }  

            this.m_TaskReportAttManager.UpdateTaskReportAtts(report,updateTaskReportModel.Atts);

            this.InternalUpdate(report);
            return report;
        }


    }
}
