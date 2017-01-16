using System;

namespace FineWork.Colla.Models
{
    public class CreateTaskTempModel
    {
        public Guid StaffId { get; set; }

        public Guid TaskId { get; set; }

        public int TempBuilds { get; set; }

        public Guid[] ToStaffs { get; set; } 
    }
}