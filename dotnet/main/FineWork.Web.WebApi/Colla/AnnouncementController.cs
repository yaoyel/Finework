using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Authorize("Bearer")]
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
        public IActionResult CreateAnnc([FromBody]CreateAnncModel anncModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, anncModel.TaskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                if (partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您没有权限创建通告.");

                var annc = this.m_AnnouncementManager.CreateAnnc(anncModel);
                var result = annc.ToViewModel();
                tx.Complete();
                return new ObjectResult(result);
            }

        }

        [HttpPost("UploadAnncAtt")]
        public IActionResult UploadAnncAtt(Guid anncId, Guid[] taskSharingIds, bool isAchv)
        {
            if(!taskSharingIds.Any()) throw new FineWorkException("请选择上传的附件.");
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc; 

                if(isAchv && annc.Staff.Account.Id!=this.AccountId)
                    throw new FineWorkException("您没有权限上传成果");

                var atts=new List<AnncAttEntity>();

                foreach (var taskSharingId in taskSharingIds.Distinct())
                {
                    var att = this.m_AnncAttManager.CreateAnncAtt(anncId, taskSharingId, isAchv);
                    atts.Add(att);
                } 
             
                var result = atts.Select(p=>p.ToViewModel());
                tx.Complete();
                return new ObjectResult(result);
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
                    throw new FineWorkException("您没有权限验收通告.");
                 
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

        [HttpPost("DeleteAnncByIds")]
        public void DeleteAnncByIds(Guid[] anncIds)
        { 
            if (anncIds.Length == 0)
                throw new FineWorkException("请传入要删除的通告Id");
            using (var tx = TxManager.Acquire())
            {

                foreach (var anncId in anncIds)
                {
                    var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;

                    var partaker = AccountIsPartakerResult.Check(annc.Task, this.AccountId).ThrowIfFailed().Partaker;

                    if (partaker.Kind != PartakerKinds.Leader)
                        throw new FineWorkException("您没有权限删除通告.");

                    this.m_AnnouncementManager.DeleteAnnc(anncId);
                }
                tx.Complete();

            }
        }

        [HttpPost("UpdateAnnc")]
        public void UpdateAnnc([FromBody]UpdateAnncModel updateAnncModel)
        {
            Args.NotNull(updateAnncModel, nameof(updateAnncModel));
            using (var tx = TxManager.Acquire())
            {
                this.m_AnnouncementManager.UpdateAnnc(updateAnncModel);
                tx.Complete();
            }

        }

        [HttpGet("FindAnncById")]
        public IActionResult FindAnncById(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).Annc;
            if(annc!=null) return new ObjectResult(annc.ToViewModel());
            return new HttpNotFoundObjectResult(anncId); 
        }

    }
}
