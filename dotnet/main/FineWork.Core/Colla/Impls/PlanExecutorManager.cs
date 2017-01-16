using System;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class PlanExecutorManager : AefEntityManager<PlanExecutorEntity, Guid>, IPlanExecutorManager
    {
        public PlanExecutorManager(ISessionProvider<AefSession> sessionProvider,
            IPlanManager planManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(planManager, nameof(planManager));
            Args.NotNull(staffManager, nameof(staffManager));
            m_PlanManager = planManager;
            m_StaffManager = staffManager;

        }

        private readonly IPlanManager m_PlanManager;
        private readonly IStaffManager m_StaffManager;
        public void CreatePlanExectors(Guid planId, Guid[] staffIds)
        {
            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan;
            if (staffIds.Any())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(m_StaffManager, staffId).Staff;
                    if (staff != null)
                    {
                        var executor = new PlanExecutorEntity();
                        executor.Id = Guid.NewGuid();
                        executor.Plan = plan;
                        executor.Staff = staff;
                        this.InternalInsert(executor);
                    } 
                }
            }
        }

        public void DeletePlanExectorsByPlanId(Guid planId)
        {
            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan;
            if (plan.Executors.Any())
            {
                foreach (var executor in plan.Executors.ToList())
                {
                    this.InternalDelete(executor);
                }
            }
        }
    }
}