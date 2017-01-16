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

        AnnouncementEntity CreateAnnc(CreateAnncModel createAnncModel, bool checkPartaker = true);

        IList<AnnouncementEntity> CreateAnncs(IList<CreateAnncModel> createAnncModel, bool checkPartaker = true);

        AnnouncementEntity FindAnncById(Guid anncId);

        IEnumerable<AnnouncementEntity> FetchAnncsByTaskId(Guid taskId);

        IEnumerable<AnnouncementEntity> FetchAnncsByStaffId(Guid staffId); 

        void DeleteAnnc(Guid anncId);

        void DeleteAnncsByTaskId(Guid taskId);

        void UpdateAnnc(UpdateAnncModel updateAnncModel);

        void CreateAnnc(AnnouncementEntity annc);

        IEnumerable<AnnouncementEntity> FetchAnncByStatus(Guid staffId,ReviewStatuses status=ReviewStatuses.Unspecified);
         
    }
}
