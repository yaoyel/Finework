using System;
using System.Collections.Generic;
using System.Linq;
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
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Anncs")]
    [Authorize]
    public class AnnouncementController : FwApiController
    {
        public AnnouncementController(ISessionProvider<AefSession> sessionProvider,
            IAnncAttManager anncAttManager,
            IAnnouncementManager announcementManager,
            IAnncIncentiveManager anncIncentiveManager,
            IPartakerManager partakerManager,
            ITaskManager taskManager) : base(sessionProvider)
        {
            Args.NotNull(anncAttManager, nameof(anncAttManager));
            Args.NotNull(announcementManager, nameof(announcementManager));
            Args.NotNull(anncIncentiveManager, nameof(anncIncentiveManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(taskManager, nameof(taskManager));

            m_TaskManager = taskManager;
            m_AnncAttManager = anncAttManager;
            m_AnncIncentiveManager = anncIncentiveManager;
            m_PartakerManager = partakerManager;
            m_AnnouncementManager = announcementManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly IAnncIncentiveManager m_AnncIncentiveManager;
        private readonly IAnncAttManager m_AnncAttManager;

        [HttpPost("CreateAnnc")]
        public IActionResult CreateAnnc(CreateAnncModel anncModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, anncModel.TaskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                if (partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您没有权限创建共识.");

                var annc = this.m_AnnouncementManager.CreateAnnc(anncModel);
                tx.Complete();
                return new ObjectResult(annc.ToViewModel());
            }

        }

        [HttpPost("UploadAnncAtt")]
        public IActionResult UploadAnncAtt(Guid anncId, Guid taskSharingId, bool isAchv)
        {
            using (var tx = TxManager.Acquire())
            {
                var anncAtt=this.m_AnncAttManager.CreateAnncAtt(anncId, taskSharingId, isAchv);
                tx.Complete();
                return new ObjectResult(anncAtt.ToViewModel());
            }
        }

        [HttpPost("DeleteAnncAtt")]
        public IActionResult DeleteAnncAtt(Guid attId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_AnncAttManager.DeleteAnncAtt(attId);
                tx.Complete();
                return new HttpStatusCodeResult(204);
            }
        }

        [HttpGet("FetchAnncsByTaskId")]
        public IActionResult FetchAnncsByTaskId(Guid taskId, int? page, int? pageSize)
        {
            var anncs = this.m_AnnouncementManager.FetchAnncsByTaskId(taskId).AsQueryable().ToPagedList(page,pageSize).ToList(); 
            
            if(anncs.Any()) return new ObjectResult(anncs.Select(p=>p.ToViewModel()));

            return new HttpNotFoundObjectResult(taskId);
        }

        [HttpPost("ChangeAnncStatus")]
        public IActionResult ChangeAnncStatus(Guid anncId, ReviewStatuses status)
        {
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;
                
                var partaker = AccountIsPartakerResult.Check(annc.Task, this.AccountId).ThrowIfFailed().Partaker;

                if (partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您没有权限验收共识.");
                 
                this.m_AnnouncementManager.ChangeAnncStatus(annc,status);
                tx.Complete();

                return new HttpStatusCodeResult(200); 
            }
        }

        [HttpPost("ChangeAnncIncentive")]
        public IActionResult ChangeAnncIncentive(Guid anncId, int incentiveKind, decimal amount)
        {
            using (var tx = TxManager.Acquire())
            {
                AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed();

                this.m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(anncId, incentiveKind, amount);
                tx.Complete();
                
                return new HttpStatusCodeResult(200);
            }
        }

    }
}
