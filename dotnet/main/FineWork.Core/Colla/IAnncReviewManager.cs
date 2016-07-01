using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IAnncReviewManager
    {
        AnncReviewEntity CreateAnncReivew(Guid anncId, ReviewStatuses reviewStatus);

        IEnumerable<AnncReviewEntity> FetchAnncReviewByAnncId(Guid anncId);

    }
}