using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IPlanManager
    {
        PlanEntity CreatePlan(CreatePlanModel planModel);

        void UpdatePlan(UpdatePlanModel planModel);

        void  UpdatePlan(PlanEntity plan);

        void DeletePlan(Guid planId);

        PlanEntity FindById(Guid planId);

        IEnumerable<PlanEntity> FetchPlansByStaffIdWithType(Guid staffId, PlanType type,bool includePrivate=true);

        IEnumerable<PlanEntity> FecthPlansByStaffId(Guid staffId);

        IEnumerable<PlanEntity> FetchPlansByContent(Guid staffId, string content);


    }
}