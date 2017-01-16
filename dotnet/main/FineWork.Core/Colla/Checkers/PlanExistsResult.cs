using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class PlanExistsResult : FineWorkCheckResult
    {
        public PlanExistsResult(bool isSucceed, String message, PlanEntity plan)
            : base(isSucceed, message)
        {
            this.Plan = plan;
        }

        public PlanEntity Plan { get; set; }
        public static PlanExistsResult Check(IPlanManager planManager, Guid planId)
        {
            var plan = planManager.FindById(planId);
            return Check(plan, "计划不存在");
        }
 

        private static PlanExistsResult Check([CanBeNull]  PlanEntity plan, String message)
        {
            if (plan == null)
            {
                return new PlanExistsResult(false, message, null);
            }

            return new PlanExistsResult(true, null, plan);
        }
    }
}
