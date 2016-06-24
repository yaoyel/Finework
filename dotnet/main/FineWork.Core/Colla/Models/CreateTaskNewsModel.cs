using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskNewsModel
    {
        public Guid TaskId { get; set; }

        public Guid StaffId { get; set; }   

        [Required(ErrorMessage ="请输入好消息内容")] 
        [MaxLength(200,ErrorMessage ="请限制在200字以内")]
        public string Message { get; set; } 
    }
}
