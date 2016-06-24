using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Security.Crypto;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using Microsoft.AspNet.Authorization;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/TaskSharings")]
    [Authorize("Bearer")]
    public class TaskSharingController: FwApiController
    {
        public TaskSharingController(ISessionProvider<AefSession> sessionProvider, ITaskSharingManager taskSharingManager)
            : base(sessionProvider)
        {
            if (taskSharingManager == null) throw new ArgumentNullException(nameof(taskSharingManager));
            m_TaskSharingManager = taskSharingManager; 
        }

        private readonly ITaskSharingManager m_TaskSharingManager;

        [HttpPost("CreateTaskSharing")]
        public IList<TaskSharingViewModel> CreateTaskSharing(IList<IFormFile> file, Guid taskId, Guid staffId)
        {
            Args.NotNull(file, nameof(file));
            var taskSharings = new List<TaskSharingViewModel>();
            file.AsParallel().ToList().ForEach(p =>
            {
                using (var reader = new StreamReader(p.OpenReadStream()))
                {
                    using (var tx = TxManager.Acquire())
                    {
                        reader.BaseStream.Position = 0;
                        var fileName = p.ContentDisposition.Split(';')[2].Split('=')[1].Replace("\"", "");
                        var taskSharing = m_TaskSharingManager.CreateTaskSharing(taskId, staffId, fileName,
                            p.ContentType, reader.BaseStream);
                        taskSharings.Add(taskSharing.ToViewModel());
                        tx.Complete();
                    }
                }
            });
            return taskSharings;
        }

        [HttpGet("FetchTaskSharingsByTask")]
        public IActionResult FetchTaskSharingsByTask(Guid taskId)
        {
            var taskSharings = this.m_TaskSharingManager.FetchTaskSharingsByTask(taskId).ToList();
            if (taskSharings.Any())
                return new ObjectResult( taskSharings.Select(p => p.ToViewModel()));
            
            return new HttpNotFoundObjectResult(taskId); 
        }

        [HttpPost("DeleteTaskSharing")]
        public IActionResult DeleteTaskSharing(Guid taskSharingId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_TaskSharingManager.DeleteTaskSharing(taskSharingId);
                tx.Complete();
                return new HttpStatusCodeResult(204);
            }
        }

        [HttpGet("DownloadTaskSharing")]
        [AllowAnonymous]
        public IActionResult DownloadTaskSharing(Guid taskSharingId)
        { 
            var taskSharing = TaskSharingExistsResult.Check(this.m_TaskSharingManager,taskSharingId).ThrowIfFailed().TaskSharing;
            var stream = new MemoryStream(); 
            this.m_TaskSharingManager.DownloadTaskSharing(taskSharing, stream);
            stream.Position = 0;
            var result= new FileStreamResult(stream,taskSharing.ContentType);
            result.FileDownloadName = taskSharing.FileName;

            return result;
        }
    }
}
