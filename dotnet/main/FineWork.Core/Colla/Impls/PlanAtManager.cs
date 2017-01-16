using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Message;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class PlanAtManager : AefEntityManager<PlanAtEntity, Guid>, IPlanAtManager
    {
        public PlanAtManager(ISessionProvider<AefSession> sessionProvider,
            IPlanManager planManager,
            IStaffManager staffManager,
            INotificationManager notificationManager,
            IConfiguration configuration
            ) : base(sessionProvider)
        {
            Args.NotNull(planManager, nameof(planManager));
            Args.NotNull(staffManager, nameof(staffManager));
            m_PlanManager = planManager;
            m_StaffManager = staffManager;
            m_Config = configuration;
            m_NotificationManager = notificationManager;
        }

        private readonly IPlanManager m_PlanManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IConfiguration m_Config;

        public List<PlanAtEntity> CreatePlanAt(Guid planId, Guid[] staffIds)
        {
            if (!staffIds.Any())
                throw new FineWorkException("请选择@的对象.");

            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan;
         
            var result = new List<PlanAtEntity>();
            foreach (var staffId in staffIds.Distinct())
            {
                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            
                var planAt = new PlanAtEntity();
                planAt.Id = Guid.NewGuid();
                planAt.Plan = plan;
                planAt.Staff = staff;
                this.InternalInsert(planAt);
                result.Add(planAt);

                SendPlanMessageAsync(plan.Creator.Name, plan.Creator.Org.Id, "planAt", plan.Id, planAt.Id,staff.Account.PhoneNumber);
            } 
            return result;
        }

        public PlanAtEntity FindByPlanIdWithStaffId(Guid planId, Guid staffId)
        {
            return this.InternalFetch(p => p.Plan.Id == planId && p.Staff.Id == staffId).FirstOrDefault();
        }

        public PlanAtEntity FindById(Guid planAtId)
        {
            return this.InternalFind(planAtId);
        }

        public IEnumerable<PlanAtEntity> FecthPlanAtsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public void UpdatePlanAt(PlanAtEntity planAt)
        {
            Args.NotNull(planAt, nameof(planAt));
            this.InternalUpdate(planAt);
        }

        public void DeletePlanAtByPlanId(Guid planId)
        {
            var ats = this.InternalFetch(p => p.Plan.Id == planId).ToList();
            if(ats.Any())
                foreach (PlanAtEntity at in ats)
                {
                    this.InternalDelete(at);
                }
        }

        public void DeletePlanAt(PlanAtEntity planAt)
        {
            if(planAt!=null)
                this.InternalDelete(planAt);
        }

        public Guid[] FetchStaffIdsByPlan(Guid planId)
        {
            var staffIds = this.InternalFetch(p => p.Plan.Id == planId).Select(p => p.Staff.Id).ToArray();
            return staffIds;
        }

        private Task SendPlanMessageAsync(string staffName,Guid orgId, string from, Guid planId, Guid planAtId,params string[] phoneNumber)
        {
            string message = string.Format(m_Config["PushMessage:Plan"], staffName);
            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "plan");
            extra.Add("From", from);
            extra.Add("OrgId", orgId.ToString()); 
            extra.Add("PlanId",planId.ToString());
            extra.Add("PlanAtId",planAtId.ToString());
            return m_NotificationManager.SendByAliasAsync("", message, extra, phoneNumber);
        }
    }
}