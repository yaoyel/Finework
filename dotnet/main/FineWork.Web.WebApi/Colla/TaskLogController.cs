using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/tasklogs")]
    [Authorize("Bearer")]
    public class TaskLogController : FwApiController
    {
        public TaskLogController(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            ITaskLogManager taskLogManager,
            IAnnouncementManager anncManager,
            ITaskNoteManager taskNoteManager
            )
            : base(sessionProvider)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (taskLogManager == null) throw new ArgumentNullException(nameof(taskLogManager));
            m_TaskManager = taskManager;
            m_TaskLogManager = taskLogManager;
            m_AnncManager = anncManager;
            m_TaskNoteManager = taskNoteManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly ITaskNoteManager m_TaskNoteManager;
        private readonly IAnnouncementManager m_AnncManager;

        [HttpGet("FetchTaskLogByTaskId")]
        public IEnumerable<TaskLogViewModel> FetchTaskLogByTaskId(Guid taskId)
        {
            return m_TaskLogManager.FetchTaskLogByTaskId(taskId)
                .Select(p => p.ToViewModel());
        }

        [HttpGet("FetchUpdateLogByTaskId")]
        public IActionResult FetchUpdateLogByTaskId(Guid taskId,string columnName, bool isLastOneOnly = true)
        {
            var result = m_TaskLogManager.FetchUpdateLogByTaskId(taskId,columnName).ToList();
            if (!result.Any())
                return new HttpNotFoundObjectResult(taskId);
            if (isLastOneOnly)
                return new ObjectResult(result.FirstOrDefault().ToViewModel());
            return new ObjectResult(result.Select(p => p.ToViewModel())); 
        }

        [HttpGet("FetchExcitationLogByTaskId")]
        public IEnumerable<TaskLogViewModel> FetchExcitationLogByTaskId(Guid taskId)
        {
            //TODO 激励的kind
            return m_TaskLogManager.FetchExcitationLogByTaskId(taskId)
                .Select(p => p.ToViewModel());
        }

        [HttpGet("FetchTaskSummarysByTaskId")]
        public IActionResult FetchTaskSummarysByTaskId(Guid taskId,DateTime? sharedAt)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var result=new List<TaskSummaryViewModel>();
            var anncs = m_AnncManager.FetchAnncsByTaskId(taskId).ToList();

            var draft = anncs.Where(p => !p.StartAt.HasValue || !p.Executors.Any() || p.Inspecter == null).ToList();

            var anncsWithMyDraft = anncs.Except(draft).Union(draft.Where(p => p.Creator.Account.Id == this.AccountId));

            var notes = m_TaskNoteManager.FetchTaskNotesByTaskId(taskId).ToList();

            if (sharedAt.HasValue)
               anncs= anncs.Where(p => p.CreatedAt <= sharedAt.Value).ToList();

            var childTasks=task.ChildTasks.ToList();

            if (anncsWithMyDraft.Any())
            {
                result.AddRange(anncs.Select(s => new TaskSummaryViewModel()
                {
                    Message = s.Content,
                    AnncStatus = GetAnnStatus(s),
                    TargetId = s.Id,
                    Type = "annc",
                    CreatedAt =s.CreatedAt,
                    EndAt = s.EndAt,
                    StartAt = s.StartAt,
                    RelatedToMe =
                        (s.Creator.Account.Id == this.AccountId ||
                         s.Executors.Any(p => p.Staff.Account.Id == this.AccountId) ||
                         s.Inspecter.Account.Id == this.AccountId),
                    IsDraft = !s.StartAt.HasValue || !s.Executors.Any() || s.Inspecter==null
                }));
            }

            if (notes.Any())
            {
                result.AddRange(notes.Select(s=>new TaskSummaryViewModel()
                {
                    Message = s.Message,
                    CreatedAt = s.CreatedAt,
                    TargetId = s.Id,
                    Type = "note"
                })); 
            }
            if (childTasks.Any())
                {
                    result.AddRange(childTasks.Select(s=>new TaskSummaryViewModel()
                    {
                        Message = $"子任务「{s.Name}」创建成功。",
                        CreatedAt = s.CreatedAt,
                        TargetId = s.Id,
                        Type = "childtask"  
                    }));
                } 
            result.Add(new TaskSummaryViewModel()
            {
                Message = "本任务创建成功。",
                CreatedAt = task.CreatedAt,
            });

            if (task.Report != null)
            {
                var report = task.Report;
                result.Add(new TaskSummaryViewModel()
                {
                    Message = "任务报告创建成功。",
                    Type = "report",
                    TargetId = report.Task.Id,
                    CreatedAt =report.CreatedAt
                });
            }

            if (task.Progress == 100)
            {
                var message = $"修改任务进度为100";
            
                //找出创建时间
                var log = m_TaskLogManager.FetchTaskLogByTaskId(taskId,true)
                    .FirstOrDefault(p => p.TargetKind.EndsWith(".Progress")
                                                                                   && p.Message == message);
                if (log != null)
                    result.Add(new TaskSummaryViewModel()
                    {
                        Message = "已完成任务。", 
                        CreatedAt =log.CreatedAt
                    });
            }
            result = result.OrderByDescending(p => p.CreatedAt).ToList();

            if (result.Any())
            {
                return new ObjectResult(result);
            }
            
            return new HttpNotFoundObjectResult(taskId);
        } 

        int? GetAnnStatus(AnnouncementEntity annc)
        {
            if (annc.StartAt > DateTime.Now)
                return -2;
             if (annc.StartAt <= DateTime.Now && annc.EndAt >DateTime.Now)
                return -1;

            var reviews = annc.Reviews.OrderByDescending(p=>p.CreatedAt).ToList();
            if (reviews.Any())
            {
                var lastReview = reviews.First();
                if (lastReview.Reviewstatus == AnncStatus.Delay && DateTime.Now >= lastReview.DelayAt)
                    return (int) AnncStatus.Unspecified;
            } 
        
               var anncStatus = annc.EndAt >= DateTime.Now
                   ? (AnncStatus?) null
                   : (annc.EndAt > DateTime.Now &&
                      !reviews.Any(
                          p => p.Reviewstatus == AnncStatus.Approved || p.Reviewstatus == AnncStatus.Abandon))
                       ? AnncStatus.Unspecified
                       : annc.Reviews.Any()
                           ? reviews.First().Reviewstatus
                           : AnncStatus.Unspecified;
               
                   return (int?) anncStatus;
        
        }
    }
}
