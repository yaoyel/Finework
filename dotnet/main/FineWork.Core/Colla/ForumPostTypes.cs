using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public enum ForumPostTypes
    {
        [Display(Description = "讨论帖")] Discussion = 1,
        [Display(Description = "共识帖")] Vote,
        [Display(Description = "投诉贴")] Complaint
    }
}
