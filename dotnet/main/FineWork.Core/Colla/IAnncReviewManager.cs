using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IAnncReviewManager
    {
        AnncReviewEntity CreateAnncReivew(Guid anncId, AnncStatus reviewStatus,DateTime? delayAt=null);

        IEnumerable<AnncReviewEntity> FetchAnncReviewByAnncId(Guid anncId);

    }
}