using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;

namespace FineWork.Colla.Impls
{
    public class IncentiveKindManager:AefEntityManager<IncentiveKindEntity,int>,IIncentiveKindManager
    {
        public IncentiveKindManager(ISessionProvider<AefSession> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public IEnumerable<IncentiveKindEntity> FetchIncentiveKind()
        {
            return this.InternalFetchAll();
        }

        public IncentiveKindEntity FindIncentiveKindById(int kindId)
        {
            return this.InternalFind(kindId);
        }
    }
}
