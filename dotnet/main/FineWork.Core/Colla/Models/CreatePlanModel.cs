using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla.Models
{
    public class UpdatePlanModel:CreatePlanModel
    {
        public Guid PlanId { get; set; }

        public new List<UpdatePlanAlarmModel> Alarms { get; set; }
    }

    public class CreatePlanModel
    {
        [MaxLength(200,ErrorMessage = "计划内容不能超过200个字.")]
        [Required(ErrorMessage = "请输入计划内容.")]
        public string Content { get; set; }

        [Range(1,3,ErrorMessage = "请选择正确的计划类型")]
        public PlanType Type { get; set; }

        public string MonthOrYear { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public Guid? InspecterId { get; set; } 

        public Guid CreatorId { get; set; }

        public Guid[] ExecutorIds { get; set; }

        public List<CreatePlanAlarmModel> Alarms { get; set; }

        public bool ExecFrPartaker { get; set; } = true;

        public Guid TaskId { get; set; }

        public Guid[] Atts { get; set; }

        public bool IsNeedAchv { get; set; } 

    }

    public class CreatePlanAlarmModel
    {
        public DateTime? Time { get; set; }

        public string BeforeStart { get; set; }

        public string Bell { get; set; }

        public Guid? PlanId { get; set; }

        public bool IsEnabled { get; set; }

        public int? BeforeStartMins
        {
            get { return AlarmTimeConverter.Converter(BeforeStart); }
        }
    }

    public class UpdatePlanAlarmModel: CreatePlanAlarmModel
    {
        public Guid? PlanAlarmId { get; set; }  
    }
}