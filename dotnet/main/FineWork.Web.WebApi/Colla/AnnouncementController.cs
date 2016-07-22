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
            ITaskManager taskManager,
            IAnncReviewManager anncReviewManager,
            IIncentiveKindManager incentiveKindManager) : base(sessionProvider)
        {
            Args.NotNull(anncAttManager, nameof(anncAttManager));
            Args.NotNull(announcementManager, nameof(announcementManager));
            Args.NotNull(anncIncentiveManager, nameof(anncIncentiveManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(anncReviewManager, nameof(anncReviewManager));
            Args.NotNull(incentiveKindManager, nameof(incentiveKindManager)); 

            m_TaskManager = taskManager;
            m_AnncAttManager = anncAttManager;
            m_AnncIncentiveManager = anncIncentiveManager;
            m_PartakerManager = partakerManager;
            m_AnnouncementManager = announcementManager;
            m_AnncReviewManager = anncReviewManager;
            m_IncentiveKindManager = incentiveKindManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly IAnncIncentiveManager m_AnncIncentiveManager;
        private readonly IAnncAttManager m_AnncAttManager;
        private readonly IAnncReviewManager m_AnncReviewManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;

        [HttpPost("CreateAnnc")]
        public IActionResult CreateAnnc([FromBody] CreateAnncModel anncModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, anncModel.TaskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                if (partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您没有权限创建里程碑.");

                var incentiveKinds = m_IncentiveKindManager.FetchIncentiveKind().Select(p=>p.ToViewModel()).ToList();
                var annc = this.m_AnnouncementManager.CreateAnnc(anncModel);
                var result = annc.ToViewModel(incentiveKinds);
                tx.Complete();
                return new ObjectResult(result);
            } 
        }

        [HttpPost("UploadAnncAtt")]
        public IActionResult UploadAnncAtt(Guid anncId, Guid[] taskSharingIds, bool isAchv)
        { 
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;

                if (isAchv && annc.Staff.Account.Id != this.AccountId)
                    throw new FineWorkException("您没有权限上传成果");

                var atts = new List<AnncAttEntity>();

                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId,true);
                foreach (var taskSharingId in taskSharingIds.Distinct())
                { 
                    var att = this.m_AnncAttManager.CreateAnncAtt(anncId, taskSharingId, isAchv);
                    atts.Add(att);
                }

                var result = atts.Select(p => p.ToViewModel());
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
            var anncs =
                this.m_AnnouncementManager.FetchAnncsByTaskId(taskId).AsQueryable().ToPagedList(page, pageSize).ToList();

            var incentiveKinds = m_IncentiveKindManager.FetchIncentiveKind().Select(p => p.ToViewModel()).ToList();
            if (anncs.Any()) return new ObjectResult(anncs.Select(p => p.ToViewModel(incentiveKinds)));

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
                    throw new FineWorkException("您没有权限验收里程碑.");

                this.m_AnncReviewManager.CreateAnncReivew(anncId, status);
                tx.Complete();

                return new HttpStatusCodeResult(200);
            }
        }

        [HttpPost("ChangeAnncIncentive")]
        public IActionResult ChangeAnncIncentive(Guid anncId, int incentiveKind, decimal amount,bool isGrant=false)
        {
            using (var tx = TxManager.Acquire())
            {
                var  annc=AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;
                if(annc.Reviews.Any(p=>p.Reviewstatus==ReviewStatuses.Approved))
                    throw new FineWorkException("激励已经兑现，不可修改.");

                this.m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(anncId, incentiveKind, amount);
                tx.Complete();

                return new HttpStatusCodeResult(200);
            }
        }

        [HttpPost("ChangeAnncIncentives")] 
        public IActionResult ChangeAnncIncetives([FromBody]UpdateAnncIncentiveModel updateAnncIncentiveModel)
        {
            Args.NotNull(updateAnncIncentiveModel, nameof(updateAnncIncentiveModel));
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, updateAnncIncentiveModel.AnncId).ThrowIfFailed().Annc;
                if (annc.Reviews.Any(p => p.Reviewstatus == ReviewStatuses.Approved))
                    throw new FineWorkException("激励已经兑现，不可修改.");
                foreach (var anncIncentive in updateAnncIncentiveModel.Incentives)
                {
                    this.m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(annc.Id, anncIncentive.Item1,
                        anncIncentive.Item2,updateAnncIncentiveModel.IsGrant);
                }
                tx.Complete();
            }
            return new HttpStatusCodeResult(200);
        }


        [HttpPost("DeleteAnncByIds")]
        public void DeleteAnncByIds(Guid[] anncIds)
        {
            if (anncIds.Length == 0)
                throw new FineWorkException("请传入要删除的里程碑Id");
            using (var tx = TxManager.Acquire())
            {

                foreach (var anncId in anncIds)
                {
                    var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;

                    if (annc.Reviews.Any(p => p.Reviewstatus != ReviewStatuses.Unspecified))
                    {
                        var anncReviewStatus = annc.Reviews.First();
                        var anncReivewStatusDesc = anncReviewStatus.Reviewstatus == ReviewStatuses.Approved
                            ? "已验收"
                            : "暂未达成";

                        throw new FineWorkException($"[{annc.Content}]处于[{anncReivewStatusDesc}]状态，不可以删除.");
                    }

                    var partaker = AccountIsPartakerResult.Check(annc.Task, this.AccountId).ThrowIfFailed().Partaker;

                    if (partaker.Kind != PartakerKinds.Leader)
                        throw new FineWorkException("您没有权限删除里程碑.");

                    this.m_AnnouncementManager.DeleteAnnc(anncId);
                }
                tx.Complete();

            }
        }

        [HttpPost("UpdateAnnc")]
        public void UpdateAnnc([FromBody] UpdateAnncModel updateAnncModel)
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
            var incentiveKinds = m_IncentiveKindManager.FetchIncentiveKind().Select(p => p.ToViewModel()).ToList();
            if (annc != null) return new ObjectResult(annc.ToViewModel(incentiveKinds));
            return new HttpNotFoundObjectResult(anncId);
        }

        [HttpGet("FetchAnncsByStatus")]
        public IActionResult FetchAnncsByStatus(Guid staffId,ReviewStatuses status = ReviewStatuses.Unspecified)
        {
            var result = this.m_AnnouncementManager.FetchAnncByStatus(staffId, status).ToList();
       

            if (result.Any()) return new ObjectResult(result.Select(p=>p.ToViewModelWithTask()));

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FindAnncWithTaskByAnncId")]
        public IActionResult FindAnncWithTaskByAnncId(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).Annc;

            if(annc!=null) return new ObjectResult(annc.ToViewModelWithTask());
            return new HttpNotFoundObjectResult(anncId);
        }

    }
}
