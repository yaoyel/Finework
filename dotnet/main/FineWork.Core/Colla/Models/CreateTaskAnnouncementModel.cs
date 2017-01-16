using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskNoteModel
    {
        public Guid TaskId { get; set; }

        public Guid StaffId { get; set; }   
         
        [Required]
        [MinLength(5,ErrorMessage ="请至少输入五个字！")]
        [MaxLength(200,ErrorMessage ="纪要不能超过200个字！" )]
        public string Message { get; set; }   

        public DateTime CreatedAt { get; set; }
    }
}
