using System;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class PlanAtNotExistsResult : FineWorkCheckResult
    {
        public PlanAtNotExistsResult(bool isSucceed, String message, PlanAtEntity planAt)
            : base(isSucceed, message)
        {
            this.PlatAt = planAt;
        }
         
        public PlanAtEntity PlatAt { get; private set; }
         
        public static PlanAtNotExistsResult Check(IPlanAtManager planAtManager, Guid planId,Guid staffId)
        {
            if (planAtManager == null) throw new ArgumentNullException(nameof(planAtManager));

            var platAt = planAtManager.FindByPlanIdWithStaffId(planId, staffId);
            return Check(platAt, $"已经存在该@对象.");
        }

        private static PlanAtNotExistsResult Check(PlanAtEntity planAt, string message)
        {
            if (planAt != null)
            {
                return new PlanAtNotExistsResult(false, message, planAt);
            }
            return new PlanAtNotExistsResult(true, null, null);
        }
    }
}