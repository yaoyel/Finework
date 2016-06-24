using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskAnnouncementManager
    {
        TaskAnnouncementEntity CreateTaskAnnouncement(CreateTaskAnnouncementModel taskAnnounceModel);

        IEnumerable<TaskAnnouncementEntity> FetchTaskAnnouncementByTaskId(Guid taskId, bool? isGoodNews,
            AnnouncementKinds announceKind = AnnouncementKinds.All);

        IEnumerable<TaskAnnouncementEntity> FetchTaskAnnouncementByStaffId(Guid staffId, bool? isGoodNews,
            AnnouncementKinds announceKind = AnnouncementKinds.All);
    }
}