using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateAnncModel
    {
        [Required(ErrorMessage = "请填写计划内容.")]
        [MaxLength(300,ErrorMessage = "计划内容长度不能大于300字符.")] 
        public string Content { get; set; }

        //老版本
        public  Guid StaffId { get; set; }
         
        //创建人
        public Guid CreatorId { get; set; }

        //执行人id
        public Guid ExecutorId { get; set; }

        //执行人ids
        public Guid[] ExecutorIds { get; set; }

        //验收人id
        public Guid? InspecterId { get; set; } 

        public Guid TaskId { get; set; }

        public List<CreateAnncAlarmModel> Alarms { get;set; }

        public  DateTime? StartAt { get; set; } 

        //结束时间
        public DateTime? EndAt { get; set; } 
      
        public Guid[] Atts { get; set; }

        public  bool IsNeedAchv { get; set; }

        public PlanType PlanType { get; set; } = PlanType.Day;

        public string MonthOrYear { get; set; } 

    }
}
