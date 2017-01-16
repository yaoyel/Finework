using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class PlanManager : AefEntityManager<PlanEntity, Guid>, IPlanManager
    {
        public PlanManager(ISessionProvider<AefSession> sessionProvider,
            ILazyResolver<IPlanAlarmManager> planAlarmManagerResolver,
            ILazyResolver<IPlanExecutorManager> planExecutorManagerResolver,
            ILazyResolver<IPlanAtManager> planAtManageResolver,
            ILazyResolver<IPushLogManager> pushLogManagerResolver,
            IStaffManager staffManager 
            ) : base(sessionProvider)
        {
            Args.NotNull(planAlarmManagerResolver, nameof(planAlarmManagerResolver));
            Args.NotNull(planExecutorManagerResolver, nameof(planExecutorManagerResolver));
            Args.NotNull(staffManager, nameof(staffManager));

            m_PlanExecutorManagerResolver = planExecutorManagerResolver;
            m_PlanAlarmManagerResolver = planAlarmManagerResolver;
            m_PlanAtManageResolver = planAtManageResolver;
            m_StaffManager = staffManager;
            m_pushLogManagerResolver = pushLogManagerResolver;
        }

        private readonly ILazyResolver<IPlanAlarmManager> m_PlanAlarmManagerResolver;
        private readonly ILazyResolver<IPlanExecutorManager> m_PlanExecutorManagerResolver;
        private readonly ILazyResolver<IPlanAtManager> m_PlanAtManageResolver;
        private readonly ILazyResolver<IPushLogManager> m_pushLogManagerResolver;
        private readonly IStaffManager m_StaffManager;

        private IPlanAlarmManager PlanAlarmManager
        {
            get { return m_PlanAlarmManagerResolver.Required; }
        }

        private IPlanExecutorManager PlanExecutorManager
        {
            get { return m_PlanExecutorManagerResolver.Required; }
        }

        private IPlanAtManager PlanAtManager
        {
            get { return m_PlanAtManageResolver.Required; } 
        }

        private IPushLogManager PushLogManager
        { 
            get { return m_pushLogManagerResolver.Required; }
        }

        public PlanEntity CreatePlan(CreatePlanModel planModel)
        {
            Args.NotNull(planModel, nameof(planModel));
            Args.NotNull(planModel, nameof(planModel));
            if (planModel.Type == PlanType.Month && string.IsNullOrEmpty(planModel.MonthOrYear))
                throw new FineWorkException("请选择正确的月份.");
            if (planModel.Type == PlanType.Year && string.IsNullOrEmpty(planModel.MonthOrYear))
                throw new FineWorkException("请选择正确的年份.");

            if (planModel.Type==PlanType.Day && (planModel.StartAt==null && planModel.EndAt==null))
                throw  new FineWorkException("请选择开始日期和结束日期.");

            var creator = StaffExistsResult.Check(this.m_StaffManager, planModel.CreatorId).ThrowIfFailed().Staff; 

            var plan = new PlanEntity();
            plan.Id = Guid.NewGuid();
            plan.Content = planModel.Content;
            plan.Creator = creator; 
            plan.StartAt = planModel.StartAt;
            plan.EndAt = planModel.EndAt;
            plan.MonthOrYear = planModel.MonthOrYear;
            plan.Type = planModel.Type;
            plan.Stars = 0; 
            plan.IsPrivate = false;
            
            plan.ExecFrPartaker = planModel.ExecFrPartaker; 

            this.InternalInsert(plan);
            if (planModel.InspecterId.HasValue && planModel.InspecterId.Value != default(Guid))
            {
                var inspecterId = StaffExistsResult.Check(this.m_StaffManager, planModel.InspecterId.Value).ThrowIfFailed().Staff;
                plan.Inspecter = inspecterId;
            }
            if (planModel.Alarms!=null && planModel.Alarms.Any())
            { 
                foreach (var alarm in planModel.Alarms)
                {
                    if (!alarm.PlanId.HasValue)
                        alarm.PlanId = plan.Id;
                    PlanAlarmManager.CreatePlanAlarm(alarm);
                }
            }
            if (planModel.ExecutorIds!=null && planModel.ExecutorIds.Any())
            { 
                PlanExecutorManager.CreatePlanExectors(plan.Id, planModel.ExecutorIds); 
            }
            return plan;
        }

        public void DeletePlan(Guid planId)
        {
            var plan = PlanExistsResult.Check(this, planId).Plan;
            if (plan != null)
            {
                PlanExecutorManager.DeletePlanExectorsByPlanId(planId);
                PlanAlarmManager.DeletePlanAlarmByPlanId(planId);
                PlanAtManager.DeletePlanAtByPlanId(planId);
                PushLogManager.DeletePushLogsByPlan(planId);
                this.InternalDelete(plan);
            }
             
        }

        public PlanEntity FindById(Guid planId)
        {
            return this.InternalFind(planId);
        }

        public void UpdatePlan(PlanEntity plan)
        {
            Args.NotNull(plan, nameof(plan));

            this.InternalUpdate(plan);
            
        }

        public void UpdatePlan(UpdatePlanModel planModel)
        {
            Args.NotNull(planModel, nameof(planModel));
            if (planModel.Type == PlanType.Month && string.IsNullOrEmpty(planModel.MonthOrYear))
                throw new FineWorkException("请选择正确的月份.");
            if (planModel.Type == PlanType.Year && string.IsNullOrEmpty(planModel.MonthOrYear))
                throw new FineWorkException("请选择正确的年份.");

            var creator = StaffExistsResult.Check(this.m_StaffManager, planModel.CreatorId).ThrowIfFailed().Staff;
     
            var plan = PlanExistsResult.Check(this, planModel.PlanId).ThrowIfFailed().Plan;

            plan.Content = planModel.Content;
            plan.Creator = creator; 
            plan.Type = planModel.Type;
            plan.StartAt = planModel.StartAt;
            plan.EndAt = planModel.EndAt;
            plan.MonthOrYear = planModel.MonthOrYear;
            plan.ExecFrPartaker = planModel.ExecFrPartaker;

            if (planModel.InspecterId.HasValue && planModel.InspecterId.Value != default(Guid))
            {
                var inspecter = StaffExistsResult.Check(this.m_StaffManager, planModel.InspecterId.Value).ThrowIfFailed().Staff;
                plan.Inspecter = inspecter;
            }
            this.InternalUpdate(plan);

            var originalExecutors = plan.Executors.Select(p => p.Id).ToArray();
            var newExecutors = planModel.ExecutorIds;

            if (originalExecutors.Except(newExecutors).Any()  || newExecutors.Except(originalExecutors).Any()) 
            {
                PlanExecutorManager.DeletePlanExectorsByPlanId(plan.Id);
                PlanExecutorManager.CreatePlanExectors(plan.Id,newExecutors);
            }

            var origianlAlarms = plan.Alarms.Select(p => p.Id).ToArray();

            var deleteAlarms =
                origianlAlarms.Except(
                    planModel.Alarms.Where(p => p.PlanAlarmId.HasValue).Select(p => p.PlanAlarmId.Value)).ToArray();
            foreach (var alarm in planModel.Alarms)
            {
                if (alarm.PlanAlarmId == null)
                    PlanAlarmManager.CreatePlanAlarm(alarm);
                if(alarm.PlanAlarmId.HasValue && origianlAlarms.Contains(alarm.PlanAlarmId.Value))
                    PlanAlarmManager.UpdatePlanAlarm(alarm);
                if(deleteAlarms.Any())
                    foreach (var alarmId in deleteAlarms)
                    {
                        PlanAlarmManager.DeletePlanAlarmByAlarmId(alarmId);
                    } 
            }
        } 

        public IEnumerable<PlanEntity> FetchPlansByStaffIdWithType(Guid staffId, PlanType type,bool includePrivate=true)
        {
            var result= this.InternalFetch(p => p.Creator.Id == staffId && p.Type == type); 
            if (!includePrivate)
                result = result.Where(p => p.IsPrivate == false).ToList();
            return result;
        }


        public IEnumerable<PlanEntity> FecthPlansByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Creator.Id == staffId);
        }

        public IEnumerable<PlanEntity> FetchPlansByContent(Guid staffId, string content)
        {
            return this.InternalFetch(p => p.Creator.Id == staffId && p.Content.Contains(content));
        }

    
    }
}