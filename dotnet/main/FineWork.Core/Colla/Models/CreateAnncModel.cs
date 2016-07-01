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

        //执行人Id
        public Guid StaffId { get; set; }


        public Guid TaskId { get; set; }

        //结束时间
        public DateTime EndAt { get; set; }

        //激励
        public List<Tuple<int,decimal>> Incentives { get; set; }

      
        public Guid[] Atts { get; set; }

        public  bool IsNeedAchv { get; set; } 

    }
}
