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
using FineWork.Files;
using FineWork.Message;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public  class MomentLikeManager:AefEntityManager<MomentLikeEntity, Guid>, IMomentLikeManager
    {
        public MomentLikeManager(ISessionProvider<AefSession> sessionProvider,
            IMomentManager mementManager,
            IStaffManager staffManager,
            IConfiguration config,
            INotificationManager notificationManager
            ) : base(sessionProvider)
        {
            Args.NotNull(mementManager, nameof(mementManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(notificationManager, nameof(notificationManager));

            m_MomentManager = mementManager;
            m_StaffManager = staffManager;
            m_Config = config;
            m_NotificationManager = notificationManager; 
        }

        private readonly IMomentManager m_MomentManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IConfiguration m_Config;
        private readonly INotificationManager m_NotificationManager;
        public MomentLikeEntity CreateMomentLike(Guid momentId, Guid staffId)
        {
            var momentLikeExistsResult = MomentLikeExistsResult.CheckByStaff(this,momentId, staffId).MomentLike;
            if (momentLikeExistsResult != null) return momentLikeExistsResult;

            var moment = MomentExistsResult.Check(this.m_MomentManager, momentId).ThrowIfFailed().Moment;
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).ThrowIfFailed().Staff;
            var momentLike = new MomentLikeEntity();
            momentLike.Id = Guid.NewGuid();
            momentLike.Moment = moment;
            momentLike.Staff = staff;

            this.InternalInsert(momentLike);

           //SendMessageWhenLikeAsync(staff, moment);

            return momentLike;
        }

        public void DeleteMomentLikeById(Guid momentLikeId)
        {
            var momentLike = this.InternalFind(momentLikeId);
            this.InternalDelete(momentLike);
        }

        public void DeleteMomentLikeByMomentId(Guid momentId)
        {
            var momentLikes = this.InternalFetch(p => p.Moment.Id == momentId);
            momentLikes.ToList().ForEach(InternalDelete);
        }

        public MomentLikeEntity FindMomentLikeById(Guid momentLikeId)
        {
            return this.InternalFind(momentLikeId);
        }

        public MomentLikeEntity FindMomentLikeByStaffId(Guid momentId, Guid staffId)
        {
            return this.InternalFetch(p => p.Moment.Id == momentId && p.Staff.Id == staffId).FirstOrDefault();
        }


        public IEnumerable<MomentLikeEntity> FetchMomentLikeByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Moment.Staff.Id == staffId && p.Staff.Id!=staffId);
        }

        private async void SendMessageWhenLikeAsync(StaffEntity staff,MomentEntity moment)
        {

            string message = string.Format(m_Config["PushMessage:Moment:Like"], staff.Name, moment.Content);

            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "moment");
            extra.Add("OrgId", moment.Staff.Org.Id.ToString());

            await m_NotificationManager.SendByAliasAsync("", message, extra, moment.Staff.Account.PhoneNumber);
        }
    }
}
