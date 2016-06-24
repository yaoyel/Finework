using System;
using System.Collections.Generic;
using FineWork.Common;

namespace FineWork.Colla
{
    public interface IPartakerReqManager
    {
        /// <summary> 创建申请. </summary>
        /// <exception cref="FineWorkException"> 当员工已经是任务的成员时. </exception>
        /// <remarks> 当申请人正被邀请时，此申请应自动被审批通过. </remarks>
        PartakerReqEntity CreatePartakerReq(TaskEntity task, StaffEntity requestorStaff, PartakerKinds kind);

        /// <summary> 更新审批状态. </summary>
        PartakerReqEntity ReviewPartakerReq(PartakerReqEntity partakerReq, ReviewStatuses reviewStatus);

        /// <summary> 根据 Id 查找 <see cref="PartakerReqEntity"/>. </summary>
        PartakerReqEntity FindPartakerReq(Guid partakerReqId);

        /// <summary> 查找任务的所有的申请（包括已经处理的与等待处理的）. </summary>
        IEnumerable<PartakerReqEntity> FetchPartakerReqsByTask(Guid taskId);

        /// <summary> 查找员工的所有的申请（包括已经处理的与等待处理的）. </summary>
        IEnumerable<PartakerReqEntity> FetchPartakerReqsByStaff(Guid staffId);

        /// <summary> 查找等待审批的申请. </summary>
        IEnumerable<PartakerReqEntity> FetchPendingPartakerReqs(Guid taskId, Guid staffId);

            /// <summary> 查找任务的等待审批的申请. </summary>
        IEnumerable<PartakerReqEntity> FetchPendingPartakerReqsByTask(Guid taskId);

        /// <summary> 查找员工的等待审批的申请. </summary>
        IEnumerable<PartakerReqEntity> FetchPendingPartakerReqsByStaff(Guid staffId);
    }
}
