using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IAnncAttManager
    {
        AnncAttEntity CreateAnncAtt(Guid anncId, Guid taskSharingId, bool isAchv);

        IEnumerable<AnncAttEntity> FetchAnncAttsByAnncId(Guid anncId, bool isAchv);

        void DeleteAnncAtt(Guid annAttId);
    }
}
