using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateMomentCommetModel
    {
        public Guid MomentId { get; set; }

        public Guid TargetCommentId { get; set; } 

        public Guid StaffId { get; set; }

        //[MaxLength(150,ErrorMessage ="评论不能超过150字")]
        [Required(ErrorMessage ="请填写评论")]
        public string Comment { get; set; }  
    }
}
