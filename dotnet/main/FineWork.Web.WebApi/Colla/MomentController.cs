using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{

    [Route("api/Moments")]
    [Authorize("Bearer")]
    
    public class MomentController : FwApiController
    { 
        public MomentController(ISessionProvider<AefSession> sessionProvider,
            IMomentManager momentManager,
            IMomentFileManager momentFileManager,
            IMomentLikeManager momentLikeManager,
            IMomentCommentManager momentCommentManager,
            IAccessTimeManager accessTimeManager,
            IStaffManager staffManager,
            IOrgManager orgManager) : base(sessionProvider)
        {
            Args.NotNull(momentManager, nameof(momentManager));
            Args.NotNull(momentFileManager, nameof(momentFileManager));
            Args.NotNull(momentManager, nameof(momentManager));
            Args.NotNull(staffManager, nameof(staffManager)); 
            Args.NotNull(accessTimeManager, nameof(accessTimeManager));
            Args.NotNull(orgManager, nameof(orgManager));

            m_MomentManager = momentManager;
            m_MomentFileManager = momentFileManager;
            m_MomentLikeManager = momentLikeManager;
            m_MomentCommentManager = momentCommentManager;
            m_SessionProvider = sessionProvider;
            m_StaffManager = staffManager;
            m_AccessTimeManager = accessTimeManager;
            m_OrgManager = orgManager;
        }
         

        private readonly ISessionProvider<AefSession> m_SessionProvider;
        private readonly IMomentManager m_MomentManager;
        private readonly IMomentFileManager m_MomentFileManager;
        private readonly IMomentLikeManager m_MomentLikeManager;
        private readonly IMomentCommentManager m_MomentCommentManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IAccessTimeManager m_AccessTimeManager;
        private readonly IOrgManager m_OrgManager;

        [HttpPost("CreateMoment")]
        public IActionResult CreateMoment([FromBody] CreateMomentModel momentModel)
        {
            Args.NotNull(momentModel, nameof(momentModel));

            using (var tx = TxManager.Acquire())
            {
                var moment = m_MomentManager.CreateMement(momentModel);
                var result = moment.ToViewModel();
                tx.Complete(); 
                return new ObjectResult(result);
            }
        }

        [HttpPost("UploadMomentFile")]
        public void UploadMomentFile(Guid momentId, IFormFile file,int count=1)
        {

            var fileName = file.ContentDisposition.Split(';')[2].Split('=')[1].Replace("\"", "");
            try
            { 
                var moment = MomentExistsResult.Check(this.m_MomentManager, momentId).ThrowIfFailed().Moment;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    m_MomentFileManager.CreateMementFile(moment.Id, file.ContentType, reader.BaseStream,fileName);  
                }
            }
            catch
            {
                this.DeleteMoment(momentId);
               
                throw new FineWorkException($"{fileName}上传失败.");
            }
        }

        [HttpPost("UploadMomentBgImage")] 
        public void UploadMomentBgImage(Guid orgId, IFormFile file)
        {  
            try
            { 
                var org = OrgExistsResult.Check(this.m_OrgManager, orgId).ThrowIfFailed().Org;
                var staff = StaffExistsResult.Check(this.m_StaffManager, org.Id, this.AccountId).ThrowIfFailed().Staff;
                PermissionIsAdminResult.Check(this.m_OrgManager, org.Id, staff.Id).ThrowIfFailed();

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    m_MomentFileManager.UploadMomentBgImage(org, file.ContentType, reader.BaseStream);
                }
            }
            catch
            { 
                throw new FineWorkException($"上传失败.");
            }
        }

        [HttpPost("DeleteMoment")]
        public IActionResult DeleteMoment(Guid momentId)
        {
            using (var tx = TxManager.Acquire())
            { 
                m_MomentCommentManager.DeleteMomentCommentByMomentId(momentId);

                m_MomentLikeManager.DeleteMomentLikeByMomentId(momentId);

                m_MomentFileManager.DeleteMementFileByMementId(momentId);
               
                m_MomentManager.DeleteMement(momentId);
                tx.Complete();

                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }
        }

        [HttpGet("FetchMomentsByOrgId")]
        public IActionResult FetchMomentsByOrgId(Guid orgId,int? page,int? pageSize)
        {
            var moments = m_MomentManager.FetchMementsByOrgId(orgId).AsQueryable().ToPagedList(page,pageSize); 

            var staff = StaffExistsResult.Check(m_StaffManager, orgId, this.AccountId).Staff;

            if (staff != null)
                m_AccessTimeManager.UpdateLastVoewMomentTime(staff.Id, DateTime.Now);

            if(!moments.Any()) return new HttpNotFoundObjectResult(orgId);

            var result = moments.Select(p => p.ToViewModel()).ToList();
            return new ObjectResult(result); 
        }

        [HttpGet("FetchMomentByStaffId")]
        public IActionResult FetchMomentByStaffId(Guid staffId, int? page, int? pageSize)
        {
            var moments = this.m_MomentManager.FetchMementsByStaffId(staffId).AsQueryable()
                .ToPagedList(page, pageSize);
            if (moments.Any())
                return new ObjectResult(moments.Select(p => p.ToViewModel()).ToList());

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpPost("CreateMomentLike")]
        public IActionResult CreateMomentLike(Guid momentId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_MomentLikeManager.CreateMomentLike(momentId, staffId);
                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.Created);
            }
        }

        [HttpPost("DeleteMomentLike")]
        public IActionResult DeleteMomentLike(Guid momentLikeId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_MomentLikeManager.DeleteMomentLikeById(momentLikeId);
                tx.Complete();
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }
        }

        [HttpPost("CreateMomentComment")]
        public IActionResult CreateMomentComment([FromBody] CreateMomentCommetModel commentModel)
        {
            using (var tx = TxManager.Acquire())
            {
                Args.NotNull(commentModel, nameof(commentModel));
                m_MomentCommentManager.CreateMomentComment(commentModel);
                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.Created);
            }
        }

        [HttpPost("DeleteMomentComment")]
        public IActionResult DeleteMomentComment(Guid commentId)
        {
            using (var tx = TxManager.Acquire())
            { 
                m_MomentCommentManager.DeleteMomentCommentById(commentId);
                tx.Complete();
                return new HttpStatusCodeResult((int)HttpStatusCode.OK);
            }
        }

        [HttpGet("HasUnReadMoment")]
        public bool HasUnReadMoment(Guid staffId)
        {
            var lastViewMomentAt = m_AccessTimeManager.FindAccessTimeByStaffId(staffId).LastViewMomentAt;
            var moments = m_MomentManager.FetchMementsByStaffId(staffId);
            if (lastViewMomentAt == null) return moments.Any();
            return moments.Any(p => p.CreatedAt > lastViewMomentAt); 
        }

        [HttpGet("FetchUnReadCommentCountByStaffId")]
        public Tuple<int,Guid?> FetchUnReadCommentCountByStaffId(Guid staffId)
        {
            var lastViewCommentAt = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime; 
            var comments = m_MomentCommentManager.FetchCommentByStaffId(staffId).ToList();
            //赞
            var likes = m_MomentLikeManager.FetchMomentLikeByStaffId(staffId).ToList();

            var unReadCount = 0;

            unReadCount = lastViewCommentAt?.LastViewCommentAt == null
                ? (comments.Count() + likes.Count())
                : (comments.Count(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt) +
                   likes.Count(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt));

            var lastComment = comments.Any()?comments.OrderByDescending(p => p.CreatedAt).FirstOrDefault():null;
            var lastLike = likes.Any()?likes.OrderByDescending(p => p.CreatedAt).FirstOrDefault():null;
            var lastCommentTime = lastComment?.CreatedAt ?? default(DateTime);
            var lastLikeTime = lastLike?.CreatedAt ?? default(DateTime);

            var lastStaff = lastCommentTime > lastLikeTime ? lastComment?.Staff.Account.Id : lastLike?.Staff.Account.Id;
            return new Tuple<int, Guid?>(unReadCount,unReadCount==0?null: lastStaff); 
        }

        [HttpGet("FetchUnReadCommentByStaffId")]
        public List<MomentCommentViewModel> FetchUnReadCommentByStaffId(Guid staffId)
        {
            var lastViewCommentAt = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime;

            //评论
            var comments = m_MomentCommentManager.FetchCommentByStaffId(staffId).ToList();

            //赞
            var likes = m_MomentLikeManager.FetchMomentLikeByStaffId(staffId).ToList(); 

            var unReadComments = lastViewCommentAt?.LastViewCommentAt == null
                ? comments.Select(p => p.ToViewModel()).ToList()
                : comments.Where(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt)
                    .ToList()
                    .Select(p => p.ToViewModel())
                    .ToList();

            var unReadLikes = lastViewCommentAt?.LastViewCommentAt == null
                ? likes.Select(p => p.ToCommentViewModel()).ToList()
                : likes.Where(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt)
                    .ToList()
                    .Select(p => p.ToCommentViewModel())
                    .ToList();

            //更新最后查看时间 
            m_AccessTimeManager.UpdateLastViewCommentTime(staffId, DateTime.Now);

            return unReadComments.Union(unReadLikes).ToList();
        }


        [HttpGet("DownloadMomentFile")]
        [AllowAnonymous]
        public IActionResult DownloadMomentFile(Guid momentFileId)
        {
            var momentFile = MomentFileExistsResult.Check(this.m_MomentFileManager, momentFileId).ThrowIfFailed().MomentFile;
            var stream = new MemoryStream();
            this.m_MomentFileManager.DownloadMomentFile(momentFile, stream);
            stream.Position = 0;
            var result = new FileStreamResult(stream, momentFile.ContentType);
            result.FileDownloadName = momentFile.Name;

            return result;
        }

        [HttpGet("FindMomentById")] 
        public IActionResult FindMomentById(Guid momentId)
        {
            var moment = MomentExistsResult.Check(this.m_MomentManager, momentId).ThrowIfFailed().Moment;
            return new ObjectResult(moment.ToViewModel());
        }
    }
}
