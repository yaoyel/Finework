using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IAnncIncentiveManager
    {
        AnncIncentiveEntity CreateOrUpdateAnncIncentive(Guid anncId,int incentiveKind, decimal amount);

        AnncIncentiveEntity FindAnncIncentiveByAnncIdAndKind(Guid anncId, int incentiveKind);

        IEnumerable<AnncIncentiveEntity> FetchAnncIncentivesByAnncId(Guid anncId);

    }
}