using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IAnnouncementManager
    {
        AnnouncementEntity CreateAnnc(CreateAnncModel createAnncModel);

        AnnouncementEntity FindAnncById(Guid anncId);

        IEnumerable<AnnouncementEntity> FetchAnncsByTaskId(Guid taskId);

        IEnumerable<AnnouncementEntity> FetchAnncsByStaffId(Guid staffId); 

        void DeleteAnnc(Guid anncId);

        void UpdateAnnc(UpdateAnncModel updateAnncModel);

        IEnumerable<AnnouncementEntity> FetchAnncByStatus(Guid staffId,ReviewStatuses status=ReviewStatuses.Unspecified);

        IEnumerable<AnnouncementEntity> FetchAnncByEndTime(DateTime endAt);


    }
}
