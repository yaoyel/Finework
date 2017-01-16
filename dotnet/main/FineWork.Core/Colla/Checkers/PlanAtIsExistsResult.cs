using System;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class PlanAtIsExistsResult : FineWorkCheckResult
    {
        public PlanAtIsExistsResult(bool isSucceed, String message, PlanAtEntity planAt)
            : base(isSucceed, message)
        {
            this.PlatAt = planAt;
        }

        public PlanAtEntity PlatAt { get; private set; }

        public static PlanAtIsExistsResult Check(IPlanAtManager planAtManager, Guid planAtId)
        {
            if (planAtManager == null) throw new ArgumentNullException(nameof(planAtManager));

            var platAt = planAtManager.FindById(planAtId);
            return Check(platAt, $"不存在该@记录.");
        }

        public static PlanAtIsExistsResult CheckForStaff(IPlanAtManager planAtManager, Guid planId,Guid staffId)
        {
            if (planAtManager == null) throw new ArgumentNullException(nameof(planAtManager));

            var platAt = planAtManager.FindByPlanIdWithStaffId(planId,staffId);
            return Check(platAt, $"不存在该@记录.");
        }

        private static PlanAtIsExistsResult Check(PlanAtEntity planAt, string message)
        {
            if (planAt == null)
            {
                return new PlanAtIsExistsResult(false, message,null);
            }
            return new PlanAtIsExistsResult(true, null, planAt);
        }
    }
}