using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using FineWork.Colla;
using Microsoft.AspNet.Mvc;
using System.Net;
using System.Text.RegularExpressions;
using FineWork.Web.WebApi.Core;
using AppBoot.Transactions;
using FineWork.Colla.Checkers;
using Microsoft.AspNet.Authorization;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Common;
using FineWork.Net.IM;
using System.Threading.Tasks;
using FineWork.Message;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Staffs")]
    [Authorize("Bearer")]
    public class StaffController : FwApiController
    {
        public StaffController(ISessionProvider<AefSession> sessionProvider, IStaffManager staffManager,
            IPartakerManager partakerManager,
            ITaskManager taskManager,
            IIMService imService,
            INotificationManager notificationManager,
            IConfiguration config,
            IAccessTimeManager accessTimeManager,
            IForumTopicManager forumTopicManager,
            IForumCommentManager forumCommentManager,
            IForumSectionManager forumSectionManager,
            IForumCommentLikeManager forumCommentLikeManager,
            IMomentManager momentManager,
            IMomentCommentManager momentCommentManager,
            IMomentLikeManager momentLikeManager,
            IAlarmManager alarmManager,
            IVoteManager voteManager,
            IAnncAlarmManager anncAlarmManager,
            ITaskVoteManager taskVoteManager)
            : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(config, nameof(config));
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(notificationManager, nameof(notificationManager));
            Args.NotNull(accessTimeManager, nameof(accessTimeManager));

            this.StaffManager = staffManager;
            this.TaskManager = taskManager;
            this.PartakerManager = partakerManager;
            this.IMService = imService;
            this.NotificationManager = notificationManager;
            this.Config = config;
            this.AccessTimeManager = accessTimeManager;

            ForumTopicManager = forumTopicManager;
            ForumCommentManager = forumCommentManager;
            ForumSectionManager = forumSectionManager;
            ForumCommentLikeManager = forumCommentLikeManager;
            MomentManager = momentManager;
            MomentCommentManager = momentCommentManager;
            MomentLikeManager = momentLikeManager;
            AlarmManager = alarmManager;
            VoteManager = voteManager;
            AnncAlarmManager = anncAlarmManager;
            TaskVoteManager = taskVoteManager;
        }

        private IStaffManager StaffManager { get; }
        private ITaskManager TaskManager { get; }
        private IIMService IMService { get; }
        private INotificationManager NotificationManager { get; }
        private IPartakerManager PartakerManager { get; }
        private IConfiguration Config { get; }
        private IAccessTimeManager AccessTimeManager { get; }
        private IForumTopicManager ForumTopicManager { get; }
        private IForumCommentManager ForumCommentManager { get; }
        private IForumSectionManager ForumSectionManager { get; }
        private IForumCommentLikeManager ForumCommentLikeManager { get; }
        private IMomentManager MomentManager { get; }
        private IMomentCommentManager MomentCommentManager { get; }
        private IMomentLikeManager MomentLikeManager { get; }
        private IAlarmManager AlarmManager { get; }
        private IVoteManager VoteManager { get; }
        private IAnncAlarmManager AnncAlarmManager { get; }
        private ITaskVoteManager TaskVoteManager { get; }

        //Match: http://localhost:41969/api/Staff/FindById?staffId=85A1ABCC-4B30-4258-87B3-F37601A97185
        [HttpGet("FindById")]
        public IActionResult FindById(Guid staffId)
        {
            var staff = this.StaffManager.FindStaff(staffId);

            return staff != null
                ? new ObjectResult(staff.ToViewModel())
                : new HttpNotFoundObjectResult(staffId);
        }

        //Match: http://localhost:41969/api/Staff/FindByOrgAndAccount?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FindByOrgAndAccount")]
        public StaffViewModel FindByOrgAndAccount(Guid orgId, Guid accountId)
        {
            var staff = this.StaffManager.FindStaffByOrgAccount(orgId, accountId);

            //更新最后一次登入组织时间
            AccessTimeManager.UpdateLastEnterOrgTime(staff.Id, DateTime.Now);
            return staff?.ToViewModel();
        }

        /* RESTful style:
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/Tom
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/
        //NotMatch: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("ByOrg/{orgId}/ByName/{staffName?}")] 
        */
        //Match: http://localhost:41969/api/Staff/FetchByOrgAndName?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&staffName=Tom
        [HttpGet("FetchByOrgAndStaffName")]
        public IEnumerable<StaffViewModel> FetchByOrgAndStaffName(Guid orgId, String staffName = null)
        {
            IEnumerable<StaffEntity> staffs = this.StaffManager.FetchStaffsByOrg(orgId, true);
            if (!String.IsNullOrEmpty(staffName))
                staffs = staffs.Where(x => x.Name == staffName);
            return staffs.Select(x => x.ToViewModel()).ToList();
        }

        //Match: http://localhost:41969/api/Staffs/FetchByOrgId?orgId=47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("FetchByOrgId")]
        public IEnumerable<StaffViewModel> FetchByOrgId(Guid orgId, bool? isEnabled)
        {
            return this.StaffManager.FetchStaffsByOrg(orgId, isEnabled)
                .Select(p => p.ToViewModel());
        }

        //Match: http://localhost:41969/api/Staffs/FetchByAccountId?accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FetchByAccountId")]
        public IEnumerable<StaffViewModel> FetchByAccountId(Guid accountId)
        {
            var staffs = this.StaffManager.FetchStaffsByAccount(accountId);
            return staffs.Select(x => x.ToViewModel()).ToList();
        }

        [HttpGet("FetchStaffsByName")]
        public IActionResult FetchStaffsByName(Guid orgId,string name,int?page,int?pageSize)
        {
            var staffs = this.StaffManager.FetchStaffsByName(orgId,name).Select(p=>p.ToViewModel()).ToList();

            if (page == null && pageSize == null)
                return new ObjectResult(staffs);
            if (staffs.Any())
               return new ObjectResult( staffs.AsQueryable().ToPagedList(page, pageSize));
            return new HttpNotFoundObjectResult(name); 

        }

        /// <summary>
        /// 员工退出组织，管理员删除员工通用此接口 
        /// </summary>
        /// <param name="staffIds"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        [HttpPost("UpdateStaffStatus")]
        //[DataScoped(true)]
        public IActionResult ChangeStaffStatus(Guid staffIds, bool newStatus)
        {
            using (var tx = TxManager.Acquire())
            {
                var staff = StaffExistsResult.Check(this.StaffManager, staffIds, false).ThrowIfFailed().Staff;

                //必须是组织管理员才可以进行此操作
                StaffExistsResult.CheckForAdmin(this.StaffManager, staff.Org.Id, this.AccountId).ThrowIfFailed();

                //判断是否有预警或共识未处理 
                if (!newStatus)
                    PartakerKindUpdateResult.Check(this.PartakerManager, staffIds).ThrowIfFailed();

                //如果是任务的负责人，管理员无权禁用
                var leader = PartakerManager.FetchPartakersByStaff(staffIds)
                    .FirstOrDefault(p => p.Kind == PartakerKinds.Leader);
                if (newStatus == false && leader != null)
                    throw new FineWorkException($"{staff.Name}在任务[{leader.Task.Name}]担任负责人,不可以禁用.");

                staff.IsEnabled = newStatus;
                this.StaffManager.UpdateStaff(staff);

                //推送消息至禁用的账号
                var attrs = new Dictionary<string, string>();
                attrs.Add("PathTo", "EnabledStaff");
                attrs.Add("Status", newStatus ? "True" : "False");
                attrs.Add("StaffId", staff.Id.ToString());
                var pushMessage = string.Format(Config["PushMessage:ChangeStaffStatus"], staff.Org.Name,
                    newStatus ? "解禁" : "禁用");
                NotificationManager.SendByAliasAsync("", pushMessage, attrs, staff.Account.PhoneNumber);

                //向其所在的任务发送即时消息
                var tasks = TaskManager.FetchTasksByStaffId(staffIds).ToList();
                if (tasks.Any())
                {
                    var imMessage = string.Format(Config["LeanCloud:Messages:Staff:ChangeStatus"], staff.Name,
                        newStatus ? "启用" : "禁用");
                    tasks.ForEach(p =>
                    {
                        IMService.SendTextMessageByConversationAsync(p.Id, this.AccountId, p.Conversation.Id.ToString(),
                            null, imMessage);
                    });

                }

                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.OK);
            }
        }

        [HttpPost("ChangeStaffName")]
        //[DataScoped(true)]
        public void ChangeStaffName(Guid staffId, string newStaffName)
        {
            Args.NotEmpty(newStaffName, nameof(newStaffName));

            Args.MaxLength(newStaffName, 18, nameof(newStaffName), "成员名称");
            var regString = @"^[\u4e00-\u9fa5a-zA-Z]{1,18}$";
            if (!Regex.IsMatch(newStaffName, regString))
            {
                throw new ArgumentException($"成员名称不允许有标点符号、数字及特殊字符");
            }

            var staff = StaffExistsResult.Check(this.StaffManager, staffId).ThrowIfFailed().Staff;

            if (staff != null && staff.Id != staffId)
            {
                throw new FineWorkException($"已经存在名称为{newStaffName}的员工.");
            }

            var oldStaffName = staff.Name;
            using (var tx = TxManager.Acquire())
            {
                staff.Name = newStaffName;
                this.StaffManager.UpdateStaff(staff);
                tx.Complete();
            }

            //修改shine会议室显示的名称
            Task.Factory.StartNew(
                async () =>
                    await IMService.ChangeConversationNameByStaffAsync(staffId.ToString(), oldStaffName, newStaffName));
        }

        [HttpPost("ChangeStaffDepartment")]
        //[DataScoped(true)]
        public void ChangeStaffDepartment(Guid staffId, string newDepartment)
        {
            if (!string.IsNullOrEmpty(newDepartment))
                Args.MaxLength(newDepartment, 32, nameof(newDepartment), "部门备注");

            using (var tx = TxManager.Acquire())
            {
                var staff = StaffExistsResult.Check(this.StaffManager, staffId).ThrowIfFailed().Staff;

                staff.Department = newDepartment;
                this.StaffManager.UpdateStaff(staff);
                tx.Complete();
            }
        }

        [HttpGet("HasUnReadMessage")]
        public IActionResult HasUnReadMessage(Guid staffId, string tabName = "")
        {
            if (tabName == "PersonalCenter")
            {
                var untreatedAlarmPeriodCount = AlarmManager.FetchUntreatedAlarmPeriodByStaff(staffId).Count();
                var voteCount = this.VoteManager.FetchVotesByStaffId(staffId)
                    .Where(p => p.EndAt <= DateTime.Now && !p.IsApproved.HasValue)
                    .Join(TaskVoteManager.FetchAllVotes(), u => u.Id, c => c.Vote.Id, (u, c) => u).Count();

                var anncs = this.AnncAlarmManager
                    .FetchAnncAlarmsByStaffId(
                        staffId)
                    .Count(w => w.Annc.EndAt <= DateTime.Now && (!w.Annc.Reviews.Any() ||
                            w.Annc.Reviews.All(p => p.Reviewstatus == AnncStatus.Delay && p.DelayAt<= DateTime.Now)));

                return new ObjectResult(untreatedAlarmPeriodCount + voteCount+ anncs);
            }

            var hasUnReadMoment = this.MomentManager.HasUnReadMoment(staffId);
            var hasUnReadComment = this.MomentManager.FetchUnReadCommentCountByStaffId(staffId).Item1 > 0;
            var unReadForumSections = this.ForumSectionManager.HasUnReadForumByStaffId(staffId);
            var hasIrrelevates = this.ForumSectionManager.FetchIrrelevantForumsByStaff(staffId,ForumSections.Mission,ForumSections.OrgGovernance,ForumSections.Strategy,
                ForumSections.Values,ForumSections.Vision).Item2;

            if (hasUnReadMoment || hasUnReadComment || hasIrrelevates.Any()|| (unReadForumSections != null && unReadForumSections.Any()))
                return
                    new ObjectResult(new Tuple<bool, ForumSections[]>(hasUnReadMoment || hasUnReadComment,
                        unReadForumSections));

            return new HttpNotFoundObjectResult(staffId);
        }
    }
}
