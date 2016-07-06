using System;

namespace FineWork.Colla.Models
{
    public class CreateForumSectionModel
    {
        public int SectionId { get; set; }

        public string Content { get; set; }

        public Guid StaffId { get; set; }
    }
}