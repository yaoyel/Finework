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
using FineWork.Message;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class MomentCommentManager : AefEntityManager<MomentCommentEntity, Guid>, IMomentCommentManager
    {
        public MomentCommentManager(ISessionProvider<AefSession> sessionProvider,
            IMomentManager mementManager,
            IStaffManager staffManager,
            IConfiguration config,
            INotificationManager notificationManager) : base(sessionProvider)
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

        public MomentCommentEntity CreateMomentComment(CreateMomentCommetModel createMomentCommetModel)
        {
            Args.NotNull(createMomentCommetModel, nameof(createMomentCommetModel));

            var moment =
                MomentExistsResult.Check(this.m_MomentManager, createMomentCommetModel.MomentId).ThrowIfFailed().Moment;

            var staff =
                StaffExistsResult.Check(this.m_StaffManager, createMomentCommetModel.StaffId).ThrowIfFailed().Staff;

            var comment = new MomentCommentEntity();
            comment.Staff = staff;
            comment.Comment = createMomentCommetModel.Comment;
            comment.Moment = moment;
            comment.Id = Guid.NewGuid();

            if (createMomentCommetModel.TargetCommentId != default(Guid))
            {
                var targetComment =
                    MomentCommentExistsResult.Check(this, createMomentCommetModel.TargetCommentId)
                        .ThrowIfFailed()
                        .MomentComment;
                comment.TargetComment = targetComment;
            }

            this.InternalInsert(comment);

            //SendMessageWhenCommentAsync(staff, moment);

            return comment;
        }

        public void DeleteMomentCommentById(Guid commentId)
        {
            var comment = MomentCommentExistsResult.Check(this, commentId).MomentComment;
            this.InternalDelete(comment);
        }

        public MomentCommentEntity FineMomentCommentById(Guid momentCommentId)
        {
            return this.InternalFind(momentCommentId);
        }

        /// <summary>
        /// 获取staff 收到的评论
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public IEnumerable<MomentCommentEntity> FetchCommentByStaffId(Guid staffId)
        {
            return
                InternalFetch(
                    p => (p.TargetComment.Staff.Id == staffId || p.Moment.Staff.Id == staffId) && p.Staff.Id != staffId);
        }

        public void DeleteMomentCommentByMomentId(Guid momentId)
        {
            var momentComments = this.InternalFetch(p => p.Moment.Id == momentId);
            momentComments.ToList().ForEach(InternalDelete);
        }


        private async void SendMessageWhenCommentAsync(StaffEntity staff, MomentEntity moment)
        {

            string message = string.Format(m_Config["PushMessage:Moment:Comment"], staff.Name, moment.Content);

            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "moment");
            extra.Add("OrgId", moment.Staff.Org.Id.ToString());

            await m_NotificationManager.SendByAliasAsync("", message, extra, moment.Staff.Account.PhoneNumber);
        }
    }
    
}
