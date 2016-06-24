using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateVoteOptionModel
    { 
       /// <summary>
        /// 选项内容
        /// </summary>
        [Required(ErrorMessage = "选项内容不能为空")] 
        [MaxLength(64,ErrorMessage ="选项内容不能大于64字符")]
        public string Content { get; set; }

        /// <summary>
        /// 是否需要填写理由
        /// </summary> 
        public bool IsNeedReason { get; set; } = false;

        public int Order { get; set; } = 1;
    }
}
