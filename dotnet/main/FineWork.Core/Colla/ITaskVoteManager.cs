using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface ITaskVoteManager
    {
        TaskVoteEntity CreateTaskVote(Guid taskId,Guid staffId,Guid voteId);

        TaskVoteEntity FineVoteById(Guid voteId);

        IEnumerable<TaskVoteEntity> FetchVoteByTaskId(Guid taskId);

        IEnumerable<TaskVoteEntity> FetchAllVotes();

        IEnumerable<TaskVoteEntity> FetchVotesByTime(DateTime time);
    }
}