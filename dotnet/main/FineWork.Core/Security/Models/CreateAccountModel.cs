using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace FineWork.Security.Models
{
    public class CreateAccountModel
    {  
        [MaxLength(18,ErrorMessage ="姓名长度不能大于18个字符")]
        [RegularExpression(@"^[\u4e00-\u9fa5a-zA-Z]{1,18}$", ErrorMessage = "姓名不允许有标点符号、数字及特殊字符")]
        public string Name { get; set; }

        public string Password { get; set; }

        [Compare("Password",ErrorMessage = "密码不一致.")]
        public string ConfirmPassword { get; set; }

        public string PhoneNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
