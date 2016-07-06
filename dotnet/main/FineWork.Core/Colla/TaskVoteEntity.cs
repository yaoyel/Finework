using System;

namespace FineWork.Colla
{
    public class TaskVoteEntity
    {
        public Guid Id { get; set; }

        public TaskEntity Task { get; set; }

        public VoteEntity Vote { get; set; } 
    }
}