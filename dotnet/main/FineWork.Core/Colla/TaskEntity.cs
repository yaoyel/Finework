using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Common;

namespace FineWork.Colla
{
    /// <summary> 代表一个任务. </summary>
    /// <remarks> see http://dev-forge.chinahrd.net/redmine/projects/finework/wiki/%E4%B8%9A%E5%8A%A1%E5%AF%B9%E8%B1%A1-%E4%BB%BB%E5%8A%A1 </remarks>
    public class TaskEntity : EntityBase<Guid>
    {
        public String Name { get; set; }

        public virtual StaffEntity Creator { get; set; }

        public virtual TaskEntity ParentTask { get; set; }

        //目标
        public string Goal { get; set; }

        //等级
        public int Level { get; set; }

        //进度
        public int Progress { get; set; } = 0;

        /// <summary> 是否允许项目成员邀请指导者. </summary>
        public bool IsMentorInvEnabled { get; set; }

        /// <summary> 是否允许项目成员邀请协作者. </summary>
        public bool IsCollabratorInvEnabled { get; set; } 

        /// <summary>否允许招募</summary>
        public bool IsRecruitEnabled { get; set; } = false;

        /// <summary>招募的角色 </summary>
        public string RecruitmentRoles { get; set; }

        /// <summary>招募说明</summary>
        public string RecruitmentDesc { get; set; }

        /// <summary>
        /// 对话id，看sdk的更新进度决定是否保留此字段
        /// </summary>
        public string ConversationId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? EndAt { get; set; }

        public virtual ICollection<PartakerEntity> Partakers { get; set; } = new HashSet<PartakerEntity>();

        public virtual ICollection<TaskEntity> ChildTasks { get; set; } = new HashSet<TaskEntity>();

        public virtual ICollection<TaskNewsEntity> Newses { get; set; } =new HashSet<TaskNewsEntity>();

        //任务承诺 
        //TODO 整理
        public virtual ICollection<TaskAnnouncementEntity> Promise { get; set; } = new HashSet<TaskAnnouncementEntity>();

        public virtual ICollection<TaskAlarmEntity> Alarms { get; set; } = new HashSet<TaskAlarmEntity>();

        public virtual ICollection<TaskLogEntity> Logs { get; set; } = new HashSet<TaskLogEntity>();

        public virtual ICollection<AlarmEntity> AlarmPeriods { get; set; } = new HashSet<AlarmEntity>();

        public virtual ICollection<TaskIncentiveEntity> Incentives { get; set; } = new HashSet<TaskIncentiveEntity>();

        public virtual ICollection<TaskVoteEntity> TaskVotes { get; set; } = new HashSet<TaskVoteEntity>();

        public virtual ICollection<TaskTransferEntity> AttachedTaskTransfers { get; set; } =
            new HashSet<TaskTransferEntity>();

        public virtual ICollection<TaskTransferEntity> DetachedTaskTransfers { get; set; } =
            new HashSet<TaskTransferEntity>();

        public virtual ICollection<TaskSharingEntity> TaskSharings { get; set; } = new HashSet<TaskSharingEntity>();

        public virtual ICollection<PartakerInvEntity> PartakerInvs { get; set; } = new HashSet<PartakerInvEntity>();

        public virtual ICollection<AnnouncementEntity> Announcements { get; set; } = new HashSet<AnnouncementEntity>();

        public virtual TaskReportEntity Report { get; set; }

        public PartakerKinds PartakerKindFor(StaffEntity staff)
        {
            var partaker = Partakers.SingleOrDefault(p => p.Staff.Id == staff.Id);
            if (partaker == null) return PartakerKinds.Unspecified;
            return partaker.Kind;
        }
    }
}