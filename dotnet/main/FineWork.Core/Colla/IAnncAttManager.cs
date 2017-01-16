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

        AnncAttEntity FindAnncAttByAnncAndSharingId(Guid anncId, Guid taskSharingId, bool isAchv);

        IEnumerable<AnncAttEntity> FetchAnncAttsBySharingId(Guid taskSharingId);
         
        void DeleteAnncAtt(Guid annAttId);

        void DeleteAnncAttByAnncId(Guid anncId,bool isAchv);

        void UpdateAnncAtts(AnnouncementEntity annc,Guid[] taskSharingIds );
    }
}
