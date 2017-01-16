using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;

namespace FineWork.Colla.Impls
{
    public class PlanAdapter : IPlanAdapter
    {

        public PlanAdapter(IPlanManager planManager,
            IAnnouncementManager anncManager)
        {
            Args.NotNull(planManager, nameof(planManager));
            Args.NotNull(anncManager, nameof(anncManager));

            m_PlanManager = planManager;
            m_AnncManager = anncManager;

        }

        private readonly IPlanManager m_PlanManager;
        private readonly IAnnouncementManager m_AnncManager;

        public dynamic CreatePlan(CreatePlanModel createPlanModel)
        {
            if (createPlanModel.TaskId != default(Guid))
                return m_AnncManager.CreateAnnc(TransformPlanToAnncModel(createPlanModel));

            return m_PlanManager.CreatePlan(createPlanModel);
        }

        public Tuple<List<AnnouncementEntity>,List<PlanEntity>> FetchPlansByStaffId(Guid staffId)
        {
            var plans = m_PlanManager.FecthPlansByStaffId(staffId).ToList();
            var anncs = m_AnncManager.FetchAnncsByStaffId(staffId).ToList();

            return new Tuple<List<AnnouncementEntity>, List<PlanEntity>>(anncs,plans);
        }

        public List<AnnouncementEntity> FetchPlansByTaskId(Guid taskId)
        {
            return m_AnncManager.FetchAnncsByTaskId(taskId).ToList();
        }

        public void DeletePlan(Guid planId)
        {
            m_AnncManager.DeleteAnnc(planId);
            m_PlanManager.DeletePlan(planId);
        }

        public void UpdatePlan(UpdatePlanModel updatePlanModel)
        {
            if (updatePlanModel.TaskId != default(Guid))
                this.m_AnncManager.UpdateAnnc(TransformPlanToAnncModel(updatePlanModel));
            else
                this.m_PlanManager.UpdatePlan(updatePlanModel);
        }

        CreateAnncModel TransformPlanToAnncModel(CreatePlanModel createPlanModel)
        {
            var result=new CreateAnncModel();
            result.EndAt = createPlanModel.EndAt;
            result.Alarms = createPlanModel.Alarms.Select(TransformPlanAlarmToAnncAlarmModel).ToList();
            result.Atts = createPlanModel.Atts;
            result.Content = createPlanModel.Content;
            result.CreatorId = createPlanModel.CreatorId;
            result.ExecutorIds = createPlanModel.ExecutorIds;
            result.InspecterId = createPlanModel.InspecterId;
            result.IsNeedAchv = createPlanModel.IsNeedAchv;
            result.MonthOrYear = createPlanModel.MonthOrYear;
            result.PlanType = createPlanModel.Type;
            result.CreatorId = createPlanModel.CreatorId;
            result.StartAt = createPlanModel.StartAt;
            result.TaskId = createPlanModel.TaskId;

            return result;
        }

        UpdateAnncModel TransformPlanToAnncModel(UpdatePlanModel updatePlanModel)
        {
            var result = TransformPlanToAnncModel( updatePlanModel);

            result.Id = updatePlanModel.PlanId; 
            result.Alarms = updatePlanModel.Alarms.Select(TransformPlanAlarmToAnncAlarmModel).ToList();
            return result; 
        }


        UpdateAnncAlarmModel TransformPlanAlarmToAnncAlarmModel(UpdatePlanAlarmModel updatePlanAlarmModel)
        {
            var result = TransformPlanAlarmToAnncAlarmModel(updatePlanAlarmModel);
            result.AnncAlarmId = updatePlanAlarmModel.PlanAlarmId;
            return result;
        }

        CreateAnncAlarmModel TransformPlanAlarmToAnncAlarmModel(CreatePlanAlarmModel createPlanAlarmModel)
        {
            var result= new CreateAnncAlarmModel();
            result.AnncId = createPlanAlarmModel.PlanId;
            result.BeforeStart = createPlanAlarmModel.BeforeStart;
            result.Bell = createPlanAlarmModel.Bell;
            result.IsEnabled = createPlanAlarmModel.IsEnabled;
            
            return result; 
        }

    }
}
