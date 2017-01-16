using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using FineWork.Message;

namespace FineWork.Web.WebApi.Colla
{
    [Authorize("Bearer")]
    [Route("api/Plans")]
    public class PlanController : FwApiController
    { 
        public PlanController(ISessionProvider<AefSession> sessionProvider,
            IPlanManager planManager,
            IStaffManager staffManager,
            IPlanAlarmManager planAlarmManager,
            IPlanAtManager planAtManager,
            IPlanExecutorManager planExecutorManager
            ) : base(sessionProvider)
        {
            Args.NotNull(planManager, nameof(planManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(planAlarmManager, nameof(planAlarmManager));
            Args.NotNull(planAtManager, nameof(planAtManager));
            Args.NotNull(planExecutorManager, nameof(planExecutorManager));

            m_PlanManager = planManager;
            m_PlanAlarmManager = planAlarmManager;
            m_StaffManager = staffManager;
            m_PlanAtManager = planAtManager;
            m_PlanExecutorManager = planExecutorManager;
        } 
        
        private readonly IPlanManager m_PlanManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IPlanExecutorManager m_PlanExecutorManager;
        private readonly IPlanAlarmManager m_PlanAlarmManager;
        private readonly IPlanAtManager m_PlanAtManager;
        private const string m_MatchStartTime = @"(\d+)([\u4e00-\u9fa5]+)(\d+)([\u4e00-\u9fa5]+)"; 

        [HttpPost("CreatePlan")]
        public IActionResult CreatePlan([FromBody]CreatePlanModel planModel)
        {
            Args.NotNull(planModel, nameof(planModel));
            using (var tx = TxManager.Acquire())
            {
                m_PlanManager.CreatePlan(planModel);
                tx.Complete();
            }

            return new HttpStatusCodeResult(201); 
        } 

        [HttpPost("UpdatePlan")]
        public IActionResult UpdatePlan([FromBody]UpdatePlanModel planModel)
        {
            Args.NotNull(planModel, nameof(planModel));
            using (var tx = TxManager.Acquire())
            {
                m_PlanManager.UpdatePlan(planModel);
                tx.Complete();
            }

            return new HttpOkResult();
        }

        [HttpPost("DeletePlan")]
        public IActionResult DeletePlan(Guid planId)
        {
            var plan = PlanExistsResult.Check(this.m_PlanManager, planId).Plan;

            if(plan!=null && plan.Creator.Account.Id!=this.AccountId)
                throw  new FineWorkException("您没有权限删除该计划.");

            using (var tx = TxManager.Acquire())
            {
                m_PlanManager.DeletePlan(planId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(204);
        }

        [HttpPost("ChangePlanStars")]
        public IActionResult ChangePlanStarts(Guid planId, int stars)
        {
            if(stars<0 || stars>5)
                throw  new FineWorkException("请现在正确的[星]值");

            using (var tx = TxManager.Acquire())
            {
                var plan = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan;
                plan.Stars = stars;
                m_PlanManager.UpdatePlan(plan);
                tx.Complete();
            }
            return new HttpOkResult();
        }

        [HttpPost("ChangePlanPrivateStatus")]
        public IActionResult ChangePlanPrivateStatus(Guid planId, bool? privateStatus)
        {
            var plan = PlanExistsResult.Check(m_PlanManager, planId).ThrowIfFailed().Plan;
            using (var tx = TxManager.Acquire())
            {
                plan.IsPrivate = privateStatus??!plan.IsPrivate;
                m_PlanManager.UpdatePlan(plan);
                tx.Complete();
            }

            return new HttpOkResult();
        }

        [HttpGet("FecthPlansByStaffIdAndType")]
        public ObjectResult FecthPlansByStaffIdAndType(Guid staffId, PlanType type, int? page, int? pageSIze,
            bool includePrivate = true)
        {

            //var allPlans = m_PlanManager.FetchPlansByStaffIdWithType(staffId, type, includePrivate).ToList();

            //if (allPlans.Any()) return new ObjectResult(allPlans.Select(p=>p.ToViewModel()));
            //return new HttpNotFoundObjectResult(staffId);

            PageList<PlanViewModel> pageList;

            List<PlanEntity> result, formerPlans = new List<PlanEntity>(), laterPlans = new List<PlanEntity>(), plans =new List<PlanEntity>();
            int firstPageNum;
            dynamic groupPlans;
            if (type == PlanType.Day)
            {
                plans = m_PlanManager.FetchPlansByStaffIdWithType(staffId, type).OrderBy(p => p.StartAt).ThenByDescending(p=>p.Stars).ToList();
                //groupPlans= plans
                //    .GroupBy(p => p.StartAt.Value.ToShortDateString()) 
                //    .Select(p => new { GroupKey = p.Key, Data = p.Select(s => s.ToViewModel()) }).ToList();

                //if (!page.HasValue && !pageSIze.HasValue && plans.Any()) return new ObjectResult(groupPlans);

                //if (page.HasValue || pageSIze.HasValue)
                //{
                //    formerPlans =
                //        plans.Where( p =>  p.StartAt.Value < DateTime.Now).OrderByDescending(p => p.StartAt).ToList();
                //    laterPlans = plans.Where(p => p.StartAt.Value >= DateTime.Now).OrderBy(p => p.StartAt).ToList();
                //}
            }

            if (type == PlanType.Month)
            {
                plans = m_PlanManager.FetchPlansByStaffIdWithType(staffId, type)
                    .OrderBy(p => int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[1].Value))
                    .ThenBy(p => int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[3].Value))
                    .ThenBy(p=>p.StartAt)
                    .ThenBy(p=>p.CreatedAt)
                    .ToList();

                //groupPlans = plans
                //    .GroupBy(p => p.MonthOrYear)
                //     .Select(p => new { GroupKey = p.Key, Data = p.Select(s => s.ToViewModel()) }).ToList();

                //if (!page.HasValue && !pageSIze.HasValue && plans.Any()) return new ObjectResult(groupPlans);

                //if (page.HasValue || pageSIze.HasValue)
                //{
                //    formerPlans =
                //        plans.Where(
                //            p =>
                //                int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[1].Value) <
                //                DateTime.Now.Year
                //                ||
                //                (int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[1].Value) ==
                //                 DateTime.Now.Year &&
                //                 int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[3].Value) <
                //                 DateTime.Now.Month)).ToList();
                //    laterPlans =
                //        plans.Where(
                //            p =>
                //                int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[1].Value) >
                //                DateTime.Now.Year
                //                ||
                //                (int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[1].Value) ==
                //                 DateTime.Now.Year &&
                //                 int.Parse(Regex.Match(p.MonthOrYear, m_MatchStartTime).Groups[3].Value) >
                //                 DateTime.Now.Month)).ToList();
                //}
            }
            if (type == PlanType.Year)
            {

                plans = m_PlanManager.FetchPlansByStaffIdWithType(staffId, type)
                    .OrderBy(p => p.MonthOrYear)
                    .ThenBy(p => p.StartAt)
                    .ThenBy(p => p.CreatedAt)
                    .ToList();

                //groupPlans = plans
                //    .GroupBy(p => p.MonthOrYear)
                //    .Select(p => new { GroupKey = p.Key, Data = p.Select(s => s.ToViewModel()) }).ToList();

                //if (!page.HasValue && !pageSIze.HasValue && plans.Any()) return new ObjectResult(groupPlans);

                //if (page.HasValue || pageSIze.HasValue)
                //{
                //    formerPlans =
                //        plans.Where(
                //            p => int.Parse(p.MonthOrYear.Substring(0, 4)) < DateTime.Now.Year).ToList();
                //    laterPlans = plans.Where(
                //        p => int.Parse(p.MonthOrYear.Substring(0, 4)) >= DateTime.Now.Year).ToList();
                //}
            }

            if (page == null && pageSIze == null)
                return new ObjectResult(plans.Select(p => p.ToViewModel()));

            return new ObjectResult(plans.Select(p => p.ToViewModel()).AsQueryable().ToPagedList(page, pageSIze).Data);

            //if (pageSIze.HasValue)
            //    plans = FillPlanList(plans, pageSIze.Value);

            //if (page == null)
            //{
            //    result = laterPlans.Any()
            //        ? laterPlans.Take(pageSIze.Value).ToList()
            //        : formerPlans.Take(pageSIze.Value).ToList();

            //    firstPageNum =int.Parse(Math.Ceiling(plans.IndexOf(result.First())/double.Parse(pageSIze.Value.ToString())).ToString());
            //    pageList = new PageList<PlanViewModel>()
            //    {
            //        Data = plans.Where(p=>p!=null).Skip((firstPageNum-1*pageSIze.Value)).Take(pageSIze.Value).Select(p=>p.ToViewModel()).ToList(),
            //        Page =  firstPageNum,
            //        PageSize = pageSIze,
            //        Total = plans.Count(p=>p!=null)
            //    };
            //}
            //else
            //{
            //    pageList = plans.Where(p => p != null)
            //        .Select(p => p.ToViewModel())
            //        .AsQueryable()
            //        .ToPagedList(page, pageSIze);
            //}

            //return new ObjectResult(pageList);
        }

        [HttpGet("FindPlanById")]
        public IActionResult FindPlanById(Guid planId,bool isAt=false)
        {
            var result = PlanExistsResult.Check(this.m_PlanManager, planId).ThrowIfFailed().Plan; 
            return new ObjectResult(result.ToDetailViewModel());
        }

        [HttpGet("FetchPlanAtsByStaffId")]
        public ObjectResult FetchPlanAtsByStaffId(Guid staffId,int?page,int?pageSize)
        {
            var result = m_PlanAtManager.FecthPlanAtsByStaffId(staffId).OrderByDescending(p=>p.CreatedAt).AsQueryable().ToPagedList(page, pageSize).Data.ToList();
            if(result.Any())
                return new ObjectResult(result.Select(p=>p.ToViewModel()));

            return new ObjectResult(result);
        }

        [HttpPost("CreatePlanAt")]
        public IActionResult CreatePlanAt(Guid planId, Guid[] staffIds)
        {
            using (var tx = TxManager.Acquire())
            {
                m_PlanAtManager.CreatePlanAt(planId, staffIds);
                tx.Complete(); 
            }

            return new HttpStatusCodeResult(201);
        }

        [HttpGet("HasUnCheckedPlanAt")]
        public IActionResult HasUnCheckedPlanAt(Guid staffId)
        {
            var planAts = m_PlanAtManager.FecthPlanAtsByStaffId(staffId).ToList();

            return new ObjectResult(planAts.Any(p=>p.IsChecked==false));
        }

        [HttpPost("ChangeCheckedStatus")]
        public IActionResult ChangeCheckedStatus(Guid planAtId)
        {
            var planAt = PlanAtIsExistsResult.Check(this.m_PlanAtManager, planAtId).ThrowIfFailed().PlatAt;

            if (!planAt.IsChecked)
            {
                using (var tx = TxManager.Acquire())
                {

                    planAt.IsChecked = true;
                    this.m_PlanAtManager.UpdatePlanAt(planAt);
                    tx.Complete();
                }
            }
            return  new HttpOkResult();
        }

        [HttpGet("FetchPlansByContent")]
        public IActionResult FetchPlansByContent(Guid staffId, string content, int? page, int? pageSize)
        {
            List<PlanEntity> plans;
            
            if (string.IsNullOrEmpty(content))
                plans = this.m_PlanManager.FecthPlansByStaffId(staffId).ToList();
            else
                plans = m_PlanManager.FetchPlansByContent(staffId, content).ToList();

            if (page == null && pageSize == null)
                return new ObjectResult(plans.Select(p=>p.ToViewModel()));
            
            var result = plans.Select(p => p.ToViewModel()).AsQueryable().ToPagedList(page, pageSize);
            return new ObjectResult(result);

        }

        [HttpPost("DeletePlanAt")]
        public IActionResult DeletePlanAt(Guid planAtId)
        {
            using (var tx = TxManager.Acquire())
            {
                var planAt = PlanAtIsExistsResult.Check(this.m_PlanAtManager, planAtId).PlatAt;
                if(planAt!=null)
                    m_PlanAtManager.DeletePlanAt(planAt); 
                tx.Complete();
            }

            return new HttpStatusCodeResult(204);
        }

        [HttpGet("FetchStaffIdsByPlan")]
        public IActionResult FetchStaffIdsByPlan(Guid planId)
        {
            var staffIds = this.m_PlanAtManager.FetchStaffIdsByPlan(planId).Distinct();
            if(staffIds.Any())
                return new ObjectResult( staffIds);
            return new HttpNotFoundObjectResult(planId);
        }

        List<PlanEntity> FillPlanList(List<PlanEntity> plans, int pageSize)
        {
            if (!plans.Any()) return plans;
            if (plans.Count %pageSize== 0)
                return plans;

            for (int i = 0; i < plans.Count%pageSize; i++)
            {
                plans.Insert(i,null);
            }
            return plans;
            
        }

    }
}