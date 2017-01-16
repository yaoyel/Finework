using System;

namespace FineWork.Colla
{
    public interface IPlanExecutorManager
    {
        void CreatePlanExectors(Guid planId, Guid[] staffIds);

        void DeletePlanExectorsByPlanId(Guid planId);
    }
}