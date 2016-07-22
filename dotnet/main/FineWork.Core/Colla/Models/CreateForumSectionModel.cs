using System;
using System.ComponentModel.DataAnnotations;

namespace FineWork.Colla.Models
{
    public class CreateForumSectionModel
    {
        public ForumSections SectionId { get; set; }

        [Required] 
        [MaxLength(200,ErrorMessage = "基本法维度的内容不能大于200字")]
        public string Content { get; set; }

        public Guid StaffId { get; set; }
    }
}