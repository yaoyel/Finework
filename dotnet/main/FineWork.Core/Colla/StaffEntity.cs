using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;
using FineWork.Security.Repos.Aef;

namespace FineWork.Colla
{
    public class StaffEntity : EntityBase<Guid>
    {
        public String Name { get; set; }

        public bool IsEnabled { get; set; }

        public string Department { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Timestamp]
        public virtual Byte[] RowVer { get; set; }

        /// <summary> Database row version for concurrency checking. </summary> 

        public virtual AccountEntity Account { get; set; }  

        public virtual OrgEntity Org { get; set; } 
        public virtual ICollection<PartakerEntity> Partakers { get; set; } =
            new HashSet<PartakerEntity>();

        public virtual ICollection<TaskLogEntity> Logs { get; set; } =
            new HashSet<TaskLogEntity>();

        public virtual ICollection<TaskNewsEntity> Newses { get; set; } =
            new HashSet<TaskNewsEntity>();


        //任务承诺
        //ToDo 整理
        public virtual ICollection<TaskAnnouncementEntity> Promise { get; set; } = new HashSet<TaskAnnouncementEntity>();


        public virtual ICollection<TaskAlarmEntity> Alarms { get; set; } =
            new HashSet<TaskAlarmEntity>();

        public virtual ICollection<AccessTimeEntity> AccessTime { get; set; } = new HashSet<AccessTimeEntity>();

        /// <summary>
        /// 创建的共识
        /// </summary>
        public virtual ICollection<VoteEntity> Votes { get; set; } = new HashSet<VoteEntity>();

        /// <summary>
        /// 参与的共识
        /// </summary>
        public virtual ICollection<VotingEntity> Votings { get; set; } = new HashSet<VotingEntity>();

        /// <summary>
        /// 已发出的激励
        /// </summary>
        public virtual ICollection<IncentiveEntity> SentIncentives { get; set; } = new HashSet<IncentiveEntity>(); 

        /// <summary>
        /// 已接收的激励
        /// </summary>
        public virtual ICollection<IncentiveEntity> ReceivedIncentives { get; set; } = new HashSet<IncentiveEntity>();

        public virtual ICollection<PartakerInvEntity> PartakerInvs { get; set; } = new HashSet<PartakerInvEntity>();

        public virtual ICollection<TaskTransferEntity> TaskTransfers { get; set; } = new HashSet<TaskTransferEntity>();

        public virtual ICollection<TaskSharingEntity> TaskSharings { get; set; } = new HashSet<TaskSharingEntity>();

        public virtual ICollection<MomentEntity> Moments { get; set; } = new HashSet<MomentEntity>();

        public virtual ICollection<MomentLikeEntity> MomentLikes { get; set; } = new HashSet<MomentLikeEntity>();

        public virtual ICollection<AnnouncementEntity> Announcements { get; set; } = new HashSet<AnnouncementEntity>();

        public virtual ICollection<MomentCommentEntity> MomentComments { get; set; } =
            new HashSet<MomentCommentEntity>();

        public virtual ICollection<ForumSectionEntity> ForumSections { get; set; }=new HashSet<ForumSectionEntity>();
         
    }
}