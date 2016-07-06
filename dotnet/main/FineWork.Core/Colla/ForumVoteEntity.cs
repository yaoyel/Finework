using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumVoteEntity:EntityBase<Guid>
    {
        public virtual ForumTopicEntity ForumTopic { get; set; }

        public virtual VoteEntity Vote { get; set; }



    }
}