using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskModel
    {
        [Required]
        [MaxLength(64, ErrorMessage = "任务名称不能大于64个字符.")]
        public string Name { get; set; }

        public Guid CreatorStaffId { get; set; }

        [MaxLength(128, ErrorMessage = "任务目标不能大于128个字符.")]

        public DateTime? EndAt { get; set; }
        public string Goal { get; set; }

        public int Level { get; set; } = 3;

        public Guid? ParentTaskId { get; set; }

        public Guid? LeaderStaffId { get; set; } 

        /// <summary> 是否允许项目成员邀请指导者. </summary>
        public bool IsMentorInvEnabled { get; set; } = false;

        /// <summary> 是否允许项目成员邀请协作者. </summary>
        public bool IsCollabratorInvEnabled { get; set; } = true;   

        public bool IsRecruitEnabled { get; set; } = false; 
        
        public int[] RecruitmentRoles { get; set; }

        [MaxLength(400, ErrorMessage = "任务招募说明不能大于400个字符.")]
        public string RecruitmentDesc { get; set; }

        //激励
        public List<CreateIncentiveModel> Incentives { get; set; }

        //定时预警
        public List<CreateAlarmPeriodModel> Alarms { get; set; }

        public  List<Guid> AlarmTempIds { get; set; }
        //接受者
        public List<Guid> Recipients { get; set; }
        
        //指导者
        public List<Guid> Mentors { get; set; }

        //协同者
        public List<Guid> Collaborators { get; set; }

        //任务模板
        public Guid? CopyFrom { get; set; }
    }  

    public class CreateIncentiveModel
    {
        public Guid? TaskId { get; set; }
        public int IncentiveKindId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreateAnncOnSharedModel
    {
        public Guid CreatorStaffId { get; set; } 

        public Guid SharedTaskId { get; set; }

        public string Name { get; set; }

        public Guid? ParentTaskId { get; set; } 

        public List<CreateAnncModel>  Anncs { get; set; }
    }
}
