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

        IEnumerable<AnnouncementEntity> FetchAnncsByStatus(ReviewStatuses reviewStatus);

        void UpdateAnnc(AnnouncementEntity annc);

        void ChangeAnncStatus(AnnouncementEntity annc, ReviewStatuses status);
    }
}
