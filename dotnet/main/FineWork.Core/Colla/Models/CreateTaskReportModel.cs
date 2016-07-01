using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskReportModel
    {
        public Guid TaskId { get; set; }

        [Required(ErrorMessage = "请填写结束时间.")]
        public DateTime EndedAt { get; set; }

        [Required(ErrorMessage = "请填写任务总结.")]
        [MaxLength(500,ErrorMessage = "任务总结不能超过500字.")]
        public string Summary { get; set; }
         
        [Range(1,5,ErrorMessage = "请对效率进行打分.")]
        public decimal EffScore { get; set; }

        [Range(1,5,ErrorMessage = "请对质量进行打分.")]
        public decimal QualityScore { get; set; }

        //表现突出的战友
        public Guid[] Exilses { get; set; }

        //附件ids
        public Guid[] Atts { get; set; }
    }
}
