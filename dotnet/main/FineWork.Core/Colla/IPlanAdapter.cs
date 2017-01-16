using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IPlanAdapter
    {
        dynamic CreatePlan(CreatePlanModel createPlanModel);

        Tuple<List<AnnouncementEntity>,List<PlanEntity>> FetchPlansByStaffId(Guid staffId);

        List<AnnouncementEntity> FetchPlansByTaskId(Guid taskId);

        void DeletePlan(Guid planId);

        void UpdatePlan(UpdatePlanModel updatePlanModel); 

    }
}