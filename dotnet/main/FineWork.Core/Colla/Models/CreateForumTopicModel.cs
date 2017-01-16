using System;
using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla.Models
{
    public class CreateForumTopicModel
    {
        public  Guid ForumSectionId { get; set; }

        public ForumPostTypes TopicType { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "讨论的内容不能大于200字")]
        [MinLength(10,ErrorMessage = "讨论的内容不能小于10个字")]
        public string Content { get; set; }
         
        [MaxLength(30,ErrorMessage = "标题内容不能超过20个字")]
        public string Title { get; set; }

        public Guid StaffId { get; set; }
    }

    public class UpdateForumTopicModel
    {
        public Guid TopicId { get; set; }

        public string Title { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "讨论的内容不能大于200字")]
        public  string Content { get; set; }

        public Guid StaffId { get; set; } 
    }
}