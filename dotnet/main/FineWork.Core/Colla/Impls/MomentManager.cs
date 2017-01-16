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
using FineWork.Core;
using FineWork.Logging;
using FineWork.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FineWork.Colla.Impls
{
    public class MomentManager:AefEntityManager<MomentEntity, Guid>, IMomentManager{
        public MomentManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,IConfiguration config,
            INotificationManager nofiNotificationManager,
            IAccessTimeManager accessTimeManager,
            ILazyResolver<IMomentCommentManager> momentCommentResolver,
            ILazyResolver<IMomentLikeManager> momentLikeResolver ) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(nofiNotificationManager, nameof(nofiNotificationManager));  
            Args.NotNull(config, nameof(config));
            m_StaffManager = staffManager;
            m_Config = config;
            m_NotificationManager = nofiNotificationManager;
            m_AccessTimeManager = accessTimeManager;
            m_MomentCommentResolver = momentCommentResolver;
            m_MomentLikeResolver = momentLikeResolver; 
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IConfiguration m_Config;
        private readonly INotificationManager m_NotificationManager;
        private readonly IAccessTimeManager m_AccessTimeManager;
        private readonly ILazyResolver<IMomentLikeManager> m_MomentLikeResolver;
        private readonly ILazyResolver<IMomentCommentManager> m_MomentCommentResolver;
        private readonly ILogger m_Logger=LogManager.GetLogger(typeof(MomentManager));
        private IMomentCommentManager MomentCommentManager
        {
            get {return m_MomentCommentResolver.Required; }
        }

        private IMomentLikeManager MomentLikeManager
        {
            get { return m_MomentLikeResolver.Required; }
        }

        public MomentEntity CreateMoment(CreateMomentModel momentModel)
        {
            Args.NotNull(momentModel, nameof(momentModel));

            var staff = StaffExistsResult.Check(m_StaffManager, momentModel.StaffId).ThrowIfFailed().Staff;
            var allStaffs = staff.Org.Staffs.Where(p => p.Id != staff.Id).ToList();
            var moment = new MomentEntity()
            {
                Id = Guid.NewGuid(),
                Type = momentModel.Type,
                Content = momentModel.Content,
                Staff = staff
            };

            this.InternalInsert(moment);

            var phoneNumbers = allStaffs.Select(p => p.Account.PhoneNumber).ToArray();

            SendMomentMessageAsync(moment, "moment", phoneNumbers);

            return moment;
        }

        public MomentEntity FindMomentById(Guid mementId)
        {
            return this.InternalFind(mementId);
        }

        public IEnumerable<MomentEntity> FetchMomentsByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId);
        }

        public IEnumerable<MomentEntity> FetchMomentsByContent(Guid orgId, string content)
        {
            if (string.IsNullOrEmpty(content))
                return this.FetchMomentsByOrgId(orgId);
            else
                return this.InternalFetch(p => p.Content.Contains(content) && p.Staff.Org.Id == orgId);
        }

        public IEnumerable<MomentEntity> FetchMomentsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public void DeleteMoment(Guid mementId)
        {
            var mement = MomentExistsResult.Check(this, mementId).ThrowIfFailed().Moment;
            this.InternalDelete(mement);
        }

      
        public  Task SendMomentMessageAsync(MomentEntity moment,string from, params string[]  phoneNumbers )
        { 
            string message = string.Format(m_Config["PushMessage:Moment"]);

            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "moment");
            extra.Add("OrgId", moment.Staff.Org.Id.ToString());
            extra.Add("From", from);
          
              return m_NotificationManager.SendByAliasAsync("", message, extra, phoneNumbers)
                .ContinueWith(
                t=>m_Logger.LogWarning(0,"momentpushwarring",t.Exception),TaskContinuationOptions.OnlyOnFaulted);
        }

        public bool HasUnReadMoment(Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            var lastViewMomentAt = m_AccessTimeManager.FindAccessTimeByStaffId(staffId).LastViewMomentAt;
            var moments = this.FetchMomentsByOrgId(staff.Org.Id).Where(p => p.Staff.Id != staff.Id);
            if (lastViewMomentAt == null) return moments.Any();
            if (moments.Any(p => p.CreatedAt > lastViewMomentAt)) return true;
            return false;
        }

        public Tuple<int, Guid?> FetchUnReadCommentCountByStaffId(Guid staffId)
        {
            var lastViewCommentAt = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime;
            var comments = MomentCommentManager.FetchCommentByStaffId(staffId).ToList();
            //赞
            var likes = MomentLikeManager.FetchMomentLikeByStaffId(staffId).ToList();

            var unReadCount = 0;

            unReadCount = lastViewCommentAt?.LastViewCommentAt == null
                ? (comments.Count() + likes.Count())
                : (comments.Count(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt) +
                   likes.Count(p => p.CreatedAt > lastViewCommentAt.LastViewCommentAt));

            var lastComment = comments.Any() ? comments.OrderByDescending(p => p.CreatedAt).FirstOrDefault() : null;
            var lastLike = likes.Any() ? likes.OrderByDescending(p => p.CreatedAt).FirstOrDefault() : null;
            var lastCommentTime = lastComment?.CreatedAt ?? default(DateTime);
            var lastLikeTime = lastLike?.CreatedAt ?? default(DateTime);

            var lastStaff = lastCommentTime > lastLikeTime ? lastComment?.Staff.Account.Id : lastLike?.Staff.Account.Id;
            return new Tuple<int, Guid?>(unReadCount, unReadCount == 0 ? null : lastStaff);
        }
    }
}
