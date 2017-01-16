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
using Microsoft.AspNet.Mvc.Abstractions;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Anncs")]
    [Authorize("Bearer")]
    public class AnnouncementController : FwApiController
    {
        public AnnouncementController(ISessionProvider<AefSession> sessionProvider,
            IAnncAttManager anncAttManager,
            IAnnouncementManager announcementManager,
            IPartakerManager partakerManager,
            ITaskManager taskManager,
            IAnncReviewManager anncReviewManager,
            IIncentiveKindManager incentiveKindManager,
            IAnncAlarmManager anncAlarmManager,
            IAnncAlarmRecManager anncAlarmRecManager) : base(sessionProvider)
        {
            Args.NotNull(anncAttManager, nameof(anncAttManager));
            Args.NotNull(announcementManager, nameof(announcementManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(anncReviewManager, nameof(anncReviewManager));
            Args.NotNull(incentiveKindManager, nameof(incentiveKindManager));

            m_TaskManager = taskManager;
            m_AnncAttManager = anncAttManager;
            m_PartakerManager = partakerManager;
            m_AnnouncementManager = announcementManager;
            m_AnncReviewManager = anncReviewManager;
            m_IncentiveKindManager = incentiveKindManager;
            m_AnncAlarmManager = anncAlarmManager;
            m_AnncAlarmRecManager = anncAlarmRecManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly IAnncAttManager m_AnncAttManager;
        private readonly IAnncReviewManager m_AnncReviewManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;
        private readonly IAnncAlarmManager m_AnncAlarmManager;
        private readonly IAnncAlarmRecManager m_AnncAlarmRecManager;

        [HttpPost("CreateAnnc")]
        public IActionResult CreateAnnc([FromBody] CreateAnncModel anncModel)
        {
            var task = TaskExistsResult.Check(m_TaskManager, anncModel.TaskId).ThrowIfFailed().Task;

            //老版本 0.9.0
            if (anncModel.StaffId != default(Guid))
            {
                var partakerForAccount = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                PartakerExistsResult.CheckForStaff(task, anncModel.StaffId).ThrowIfFailed();
                anncModel.CreatorId = partakerForAccount.Staff.Id;
                anncModel.ExecutorId = anncModel.StaffId;
                anncModel.InspecterId = partakerForAccount.Staff.Id;
            }

            //执行人一人变多人
            if (anncModel.ExecutorId != default(Guid) && anncModel.ExecutorIds == null)
            {
                anncModel.ExecutorIds=new Guid[] {anncModel.ExecutorId};
            } 
            
            if(anncModel.StartAt.HasValue && anncModel.EndAt.HasValue)
                if(anncModel.EndAt.Value<anncModel.StartAt.Value)
                    throw  new FineWorkException("结束时间不能小于开始时间.");

            using (var tx = TxManager.Acquire())
            {
                var annc = this.m_AnnouncementManager.CreateAnnc(anncModel);
                var result = annc.ToViewModel();
                tx.Complete();
                return new ObjectResult(result);
            }
        }

        /// <summary>
        /// 复制任务计划
        /// </summary>
        /// <param name="anncModels"></param>
        /// <returns></returns>
        [HttpPost("CreateAnncs")]
        public IActionResult CreateAnnc([FromBody] List<CreateAnncModel> anncModels)
        {  
            List<AnncViewModel> result;
            using (var tx = TxManager.Acquire())
            {
                var anncs=m_AnnouncementManager.CreateAnncs(anncModels, false);
                result = anncs.Select(p => p.ToViewModel()).ToList();
                tx.Complete();
            }
            return new ObjectResult(result);
        }

        [HttpPost("UploadAnncAtt")]
        public IActionResult UploadAnncAtt(Guid anncId, Guid[] taskSharingIds, bool isAchv)
        {
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;

                if (isAchv && annc.Inspecter.Account.Id != this.AccountId && annc.Executors.All(p => p.Staff.Account.Id != this.AccountId))
                    throw new FineWorkException("您没有权限上传成果");

                var atts = new List<AnncAttEntity>();

                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId, true);
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
                this.m_AnnouncementManager.FetchAnncsByTaskId(taskId).AsQueryable().ToPagedList(page, pageSize).Data.ToList();

            if (anncs.Any()) return new ObjectResult(anncs.Select(p => p.ToViewModel()));

            return new HttpNotFoundObjectResult(taskId);
        }

        [HttpPost("ChangeAnncStatus")]
        public IActionResult ChangeAnncStatus(Guid anncId, AnncStatus status,DateTime? delayAt)
        {
            using (var tx = TxManager.Acquire())
            {
                var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;


               if( annc.Inspecter.Account.Id != this.AccountId) 
                throw new FineWorkException("您没有权限验收该计划.");

                this.m_AnncReviewManager.CreateAnncReivew(anncId, status,delayAt);
                tx.Complete();

                return new HttpStatusCodeResult(200);
            }
        } 

        [HttpPost("DeleteAnncByIds")]
        public void DeleteAnncByIds(Guid[] anncIds)
        {
            if (anncIds.Length == 0)
                throw new FineWorkException("请传入要删除的计划Id");
            using (var tx = TxManager.Acquire())
            {

                foreach (var anncId in anncIds)
                {
                    var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;

                    if (annc.Reviews.Any(p => p.Reviewstatus == AnncStatus.Approved))
                    { 
                        throw new FineWorkException($"计划处于[已验收]状态，不可以删除.");
                    }

                    var partaker = AccountIsPartakerResult.Check(annc.Task, this.AccountId).ThrowIfFailed().Partaker;

                    if (partaker.Staff.Id != annc.Creator.Id)
                        throw new FineWorkException("您没有权限删除计划.");

                    this.m_AnnouncementManager.DeleteAnnc(anncId);
                }
                tx.Complete();

            }
        }

        [HttpPost("UpdateAnnc")]
        public void UpdateAnnc([FromBody] UpdateAnncModel updateAnncModel)
        {
            if (updateAnncModel.ExecutorId != default(Guid) && updateAnncModel.ExecutorIds == null)
            {
                updateAnncModel.ExecutorIds = new Guid[] { updateAnncModel.ExecutorId };
            }

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
            if (annc != null) return new ObjectResult(annc.ToViewModel());
            return new HttpNotFoundObjectResult(anncId);
        }

        [HttpGet("FetchAnncsByStatus")]
        public IActionResult FetchAnncsByStatus(Guid staffId, ReviewStatuses status = ReviewStatuses.Unspecified)
        {
            var result = this.m_AnnouncementManager.FetchAnncByStatus(staffId, status).ToList();


            if (result.Any()) return new ObjectResult(result.Select(p => p.ToViewModelWithTask()));

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FindAnncWithTaskByAnncId")]
        public IActionResult FindAnncWithTaskByAnncId(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).Annc;

            if (annc != null) return new ObjectResult(annc.ToViewModelWithTask());
            return new HttpNotFoundObjectResult(anncId);
        }

        [HttpPost("CreateAnncAlarm")]
        public IActionResult CreateAnncAlarm(CreateAnncAlarmModel anncAlarmModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var alarm = m_AnncAlarmManager.CreateAnnAlarm(anncAlarmModel);
                var result = alarm.ToViewModel();
                tx.Complete();
                return new HttpOkResult();
            }
        }

        [HttpPost("DeleteAnncAlarm")]
        public IActionResult DeleteAnncAlarm(Guid anncAlarmId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_AnncAlarmManager.DeleteAnncAlarm(anncAlarmId);
                tx.Complete();
                return new HttpOkResult();
            }
        }

        [HttpPost("UpdateAnncAlarm")]
        public IActionResult UpdateAnncAlarm(UpdateAnncAlarmModel anncAlarmModel)
        {
            using (var tx = TxManager.Acquire())
            {
                m_AnncAlarmManager.UpdateAnncAlarm(anncAlarmModel);
                tx.Complete();
                return new HttpOkResult();
            }
        }

        [HttpGet("IsAlarmReceiver")]
        public bool IsAlarmReceiver(Guid anncId, Guid staffId, AnncRoles role)
        {
            return m_AnncAlarmRecManager.FetchRecsByAnncIdWithStaffId(anncId, staffId, role) != null;
        }

        [HttpPost("UpdateAnncStatus")]
        public IActionResult UpdateAnncStatus(Guid anncAlarmId, bool isEnabled)
        {
            using (var tx = TxManager.Acquire())
            {
                m_AnncAlarmManager.UpdateAnncAlarmStatus(anncAlarmId,isEnabled);
                tx.Complete();
            }
       
            return new HttpOkResult();
        }
     }
}
