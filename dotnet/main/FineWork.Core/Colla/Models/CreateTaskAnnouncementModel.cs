using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskAnnouncementModel
    {
        public Guid TaskId { get; set; }

        public Guid StaffId { get; set; }  
 
        [Required]
        [Range(1,2,ErrorMessage ="请确认创建通告的类型！")]
        public AnnouncementKinds AnnouncementKind { get; set; }

        public bool IsGoodNews { get; set; } = false;

        [Required]
        [MinLength(5,ErrorMessage ="请至少输入五个字！")]
        public string Message { get; set; } 

    }
}
