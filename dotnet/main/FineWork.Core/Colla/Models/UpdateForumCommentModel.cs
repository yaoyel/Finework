using System;

namespace FineWork.Colla.Models
{
    public class UpdateForumCommentModel
    {
        public Guid StaffId { get; set; }

        public string Comment { get; set; }

        public Guid CommentId { get; set; }
    }
}