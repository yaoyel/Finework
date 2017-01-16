using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/tasknotes")]
    [Route("api/Taskannouncements")]
    [Authorize("Bearer")]
    public class TaskNoteController : FwApiController
    {
        public TaskNoteController(ISessionProvider<AefSession> sessionProvider,
            ITaskNoteManager taskNoteManager)
            : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentNullException(nameof(sessionProvider));
            if (taskNoteManager == null) throw new ArgumentNullException(nameof(taskNoteManager));
            m_TaskNoteManager = taskNoteManager;
        }

        private readonly ITaskNoteManager m_TaskNoteManager;

        [HttpPost("CreateTaskNote")]
        [HttpPost("CreateTaskAnnouncement")]
        //[DataScoped(true)]
        public TaskNoteViewModel CreateTaskNote([FromBody]CreateTaskNoteModel taskNoteModel)
        {
            using (var tx = TxManager.Acquire())
            {
                if (taskNoteModel == null) throw new ArgumentNullException(nameof(taskNoteModel));

                var announcement = m_TaskNoteManager.CreateTaskNote(taskNoteModel);
                var result = announcement.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FetchTaskNotesByTaskId")]
        [HttpGet("FetchTaskAnnouncementsByTaskId")]
        public IEnumerable<TaskNoteViewModel> FetchTaskNotesByTaskId(Guid taskId)
        {
            return m_TaskNoteManager.FetchTaskNotesByTaskId(taskId)
                .Select(p => p.ToViewModel()).ToList();
        }

        [HttpGet("FindTaskNoteById")]
        public IActionResult FindTaskNoteById(Guid noteId)
        {
            var result = m_TaskNoteManager.FindTaskNoteById(noteId);

            if(result==null)return new HttpNotFoundObjectResult(noteId);
            return new ObjectResult(result.ToViewModel());
        }

        [HttpPost("DeleteTaskNote")]
        public IActionResult DeleteTaskNote(Guid noteId)
        {
            var taskNote = TaskNoteExistsResult.Check(this.m_TaskNoteManager, noteId).TaskNote;
            using (var tx = TxManager.Acquire())
            {
                this.m_TaskNoteManager.DeleteTaskNote(taskNote);
                tx.Complete();
            }
            return new HttpStatusCodeResult(204); 

        }

        [HttpPost("UpdateTaskNote")]
        public IActionResult UpdateTaskNote(Guid noteId, string message,DateTime? createdAt)
        {
            Args.NotEmpty(message, nameof(message));
            if(message.Length>200) throw new FineWorkException("纪要不能超过200个字.");
            var taskNote = TaskNoteExistsResult.Check(this.m_TaskNoteManager, noteId).ThrowIfFailed().TaskNote;

            using (var tx = TxManager.Acquire())
            {
                taskNote.Message = message;

                if (createdAt.HasValue)
                    taskNote.CreatedAt = createdAt.Value;
                this.m_TaskNoteManager.UpdateTaskNote(taskNote);
                tx.Complete();
            }
            return new HttpStatusCodeResult(200);
        }
    }
}
