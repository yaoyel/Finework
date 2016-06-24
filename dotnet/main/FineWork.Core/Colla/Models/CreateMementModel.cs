using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateMomentModel
    {
        [MaxLength(500,ErrorMessage ="共享的内容不能超过300个字符")]
        public string Content { get; set; }

        public Guid StaffId { get; set; }

        public MomentType Type { get; set; } = MomentType.Word; 
    }
}
