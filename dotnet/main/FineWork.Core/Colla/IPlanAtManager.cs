using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IPlanAtManager
    {
        List<PlanAtEntity> CreatePlanAt(Guid planId, Guid[] staffId);

        PlanAtEntity FindByPlanIdWithStaffId(Guid planId, Guid staffId);

        PlanAtEntity FindById(Guid planAtId);

        IEnumerable<PlanAtEntity> FecthPlanAtsByStaffId(Guid staffId);

        void UpdatePlanAt(PlanAtEntity planAt);

        void DeletePlanAtByPlanId(Guid planId);

        void DeletePlanAt(PlanAtEntity planAt);

        Guid[] FetchStaffIdsByPlan(Guid planId);
    }
}