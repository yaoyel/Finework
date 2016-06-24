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
        [Required(ErrorMessage = "请填写公告内容.")]
        [MaxLength(300,ErrorMessage = "公告内容长度不能大于300字符.")] 
        public string Content { get; set; }

        //执行人
        public Guid StaffId { get; set; }


        public Guid TaskId { get; set; }

        public DateTime EndAt { get; set; }

        //激励
        public IDictionary<int,decimal> Incentives { get; set; }

        //资源
        public Guid[] Atts { get; set; }

        public  bool IsNeedAchv { get; set; } 

    }
}
