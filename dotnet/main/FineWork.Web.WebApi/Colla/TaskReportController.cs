﻿using System;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/TaskReports")]
    [Authorize("Bearer")]
    public class TaskReportController : FwApiController
    {
        public TaskReportController(ISessionProvider<AefSession> sessionProvider,
            ITaskReportManager taskReportManager,
            ITaskReportAttManager taskReportAttManager,
            ITaskManager taskManager,
            IPartakerManager partakerManager)
            : base(sessionProvider)
        {
            Args.NotNull(taskReportManager, nameof(taskReportManager));
            Args.NotNull(taskReportAttManager, nameof(taskReportAttManager));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(partakerManager, nameof(partakerManager));

            m_TaskManager = taskManager;
            m_PartakerManager = partakerManager;
            m_TaskReportAttManager = taskReportAttManager;
            m_TaskReportManager = taskReportManager;
        }

        private readonly ITaskReportManager m_TaskReportManager;
        private readonly ITaskReportAttManager m_TaskReportAttManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;

        [HttpPost("CreateTaskReport")]
        public IActionResult CreateTaskReport([FromBody] CreateTaskReportModel taskReportModel)
        {
            Args.NotNull(taskReportModel, nameof(taskReportModel));
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, taskReportModel.TaskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker; 

                if(partaker.Kind!=PartakerKinds.Leader)
                    throw new FineWorkException("你没有权限结束任务.");

                //判断是否有预警或共识未处理 
                AlarmOrVoteExistsResult.Check(this.m_TaskManager, taskReportModel.TaskId).ThrowIfFailed();

                this.m_TaskReportManager.CreateTaskReport(taskReportModel);
      
                tx.Complete();
                return new HttpStatusCodeResult(201);
            } 
          
        }

        [HttpPost("FindTaskReportByTaksId")]
        public IActionResult FindTaskReportByTaksId(Guid taskId)
        {
            var report = this.m_TaskReportManager.FindTaskReportByTaskId(taskId);
            
            if(report==null) return  new HttpNotFoundObjectResult(taskId);

            return new ObjectResult(report.ToViewModel());
        }

        [HttpPost("DeleteTaskReportAttById")]
        public IActionResult DeleteTaskReportAttById(Guid taskReportAttId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_TaskReportAttManager.DeleteTaskReportAtt(taskReportAttId);
                tx.Complete();
            }
           
            return  new HttpStatusCodeResult(204); 
        }

        [HttpPost("UpdateTaskReport")]
        public IActionResult UpdateTaskReport(UpdateTaskReportModel updateTaskReportModel)
        {
            Args.NotNull(updateTaskReportModel, nameof(updateTaskReportModel));
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, updateTaskReportModel.TaskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                if (partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("你没有权限修改报告.");

                this.m_TaskReportManager.UpdateTaskReport(updateTaskReportModel);

                tx.Complete();
                return new HttpStatusCodeResult(201);
            }
        } 
    }
}