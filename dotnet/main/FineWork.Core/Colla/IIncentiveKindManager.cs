using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IIncentiveKindManager
    {
        IEnumerable<IncentiveKindEntity> FetchIncentiveKind();

        IncentiveKindEntity FindIncentiveKindById(int kindId);
    }
}
