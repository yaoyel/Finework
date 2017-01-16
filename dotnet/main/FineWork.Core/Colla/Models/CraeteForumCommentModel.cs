using System;
using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla.Models
{
    public class CraeteForumCommentModel
    {
       public  Guid StaffId { get; set; }

        public  Guid TopicId { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "评论的内容不能大于200字")]
        public string Comment { get; set; }

        public Guid TargetCommentId { get; set; }
    }
}