using System;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskVoteEntity:EntityBase<Guid>
    { 

        public virtual VoteEntity Vote { get; set; }

        public virtual TaskEntity Task { get; set; } 
    }
}