using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IPartakerManager
    {
        PartakerEntity CreatePartaker(Guid taskId, Guid staffId, PartakerKinds kind, bool isSendMessage = true);

        PartakerEntity CreateCollabrator(Guid taskId, Guid staffId);

        PartakerEntity CreateRecipient(Guid taskId, Guid staffId);

        PartakerEntity CreateMentor(Guid taskId, Guid staffId);

        PartakerEntity RemoveCollabrator(Guid taskId, Guid staffId);

        PartakerEntity RemoveMentor(Guid taskId, Guid staffId);

        PartakerEntity RemoveRecipient(Guid taskId, Guid staffId);

        PartakerEntity ChangeLeader(Guid taskId, Guid staffId); 

        PartakerEntity ExitTask(Guid taskId, Guid staffId);

        PartakerEntity FindPartaker(Guid partakerId);

        PartakerEntity FindPartaker(Guid taskId, Guid staffId);

        IEnumerable<PartakerEntity> FetchPartakersByTask(Guid taskId);

        IEnumerable<PartakerEntity> FetchPartakersByStaff(Guid staffId);

        PartakerEntity ChangePartakerKind(TaskEntity task, StaffEntity staff, PartakerKinds partakerKind);

        void UpdatePartaker(PartakerEntity partaker); 

        IEnumerable<PartakerEntity> FetchExilsesByTaskId(Guid taskId);
         
    }
}
