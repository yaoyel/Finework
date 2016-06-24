using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IPartakerInvManager
    {
        /// <summary> 直接将人员加入到任务中. </summary>
        /// <param name="task"> 任务. </param>
        /// <param name="inviterStaff"> 邀请人 </param>
        /// <param name="inviteeStaff"> 受邀人 </param>
        /// <param name="partakerKind"> 任务角色 </param>
        /// <remarks> 适用于邀请他人加入任务时不需要被邀请人审批的场合. </remarks>
        PartakerInvEntity QuickAdd(TaskEntity task, StaffEntity inviterStaff, StaffEntity inviteeStaff, PartakerKinds partakerKind); 

        /// <summary> 更新审批状态. </summary> 
        PartakerInvEntity ReviewPartakerInv(PartakerInvEntity partakerInvEntity, ReviewStatuses newRevStatus);

        /// <summary> 根据 Id 查找 <see cref="PartakerInvEntity"/>. </summary>
        PartakerInvEntity FindPartakerInv(Guid partakerInvId);

        /// <summary> 查找正在审批的邀请. </summary>
        /// <param name="taskId"> 任务 <see cref="TaskEntity.Id"/>. </param>
        /// <param name="staffId"> 被邀请的员工的 <see cref="TaskEntity.Id"/>. </param>
        IEnumerable<PartakerInvEntity> FetchPendingPartakerInvs(Guid taskId, Guid staffId);

        /// <summary> 查找任务的等待审批的邀请. </summary>
        IEnumerable<PartakerInvEntity> FetchPendingPartakerInvsByTask(Guid taskId);

        /// <summary> 查找员工的等待审批的邀请. </summary>
        IEnumerable<PartakerInvEntity> FetchPendingPartakerInvsByStaff(Guid staffId);

        /// <summary> 查找已经发送的邀请 </summary> 
        IEnumerable<PartakerInvEntity> FetchSentPartakerInvsByStaff(Guid staffId);
    }
}
