using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;
using FineWork.Security.Repos.Aef;
using FineWork.Web.WebApi.Colla;
using FineWork.Web.WebApi.Tests.Core;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace FineWork.Web.WebApi.Tests.Colla
{
    [Collection(FwApiBed.Name)]
    public class PartakerInvControllerTests : FwApiTestBase
    {
        public PartakerInvControllerTests(FwApiBed bed)
            : base(bed)
        {
        }

        [Fact]
        public void QuickAdd_adds_mentor_when_adding_mentor_and_IsMentorInvEnabled_is_true()
        {
            SetupTask(delegate (
                PartakerInvController invController,
                AefSession session,
                IPartakerInvManager invManager,
                OrgEntity org, TaskEntity task,
                StaffEntity leader, StaffEntity collabrator, StaffEntity mentor, StaffEntity recipient
                )
            {
                task.IsMentorInvEnabled = true;
                session.SaveChanges();

                invController.InternalSetAccount(leader.Account);

                var invitee = this.Services.CreateAccountStaff(org);
                var createPartakerInvModel = new CreatePartakerInvModel()
                {
                    TaskId=task.Id,
                    InviteeStaffIds = new[] {invitee.Id},
                    PartakerKind=PartakerKinds.Mentor
                };
                
                var model = invController.QuickAdd(createPartakerInvModel).First();

                Assert.NotNull(model);
                Assert.Equal(task.Id, model.Task.Id);
                Assert.Equal(invitee.Id, model.Staff.Id);
                Assert.Equal(PartakerKinds.Mentor, model.PartakerKind);
                Assert.Equal(ReviewStatuses.Approved, model.ReviewStatus);
            });
        }

        [Fact]
        public void QuickAdd_adds_mentor_when_adding_mentor_by_leader_and_IsMentorInvEnabled_is_false()
        {
            SetupTask(delegate(
                PartakerInvController invController,
                AefSession session,
                IPartakerInvManager invManager,
                OrgEntity org, TaskEntity task,
                StaffEntity leader, StaffEntity collabrator, StaffEntity mentor, StaffEntity recipient
                )
            {
                task.IsMentorInvEnabled = false;
                session.SaveChanges();

                invController.InternalSetAccount(leader.Account);

                var invitee = this.Services.CreateAccountStaff(org);
                 var createPartakerInvModel = new CreatePartakerInvModel()
                {
                    TaskId=task.Id,
                    InviteeStaffIds = new[] {invitee.Id},
                    PartakerKind=PartakerKinds.Mentor
                };
                
                var model = invController.QuickAdd(createPartakerInvModel).First();

                Assert.NotNull(model);
                Assert.Equal(task.Id, model.Task.Id);
                Assert.Equal(invitee.Id, model.Staff.Id);
                Assert.Equal(PartakerKinds.Mentor, model.PartakerKind);
                Assert.Equal(ReviewStatuses.Approved, model.ReviewStatus);
            });
        }

        private delegate void QuickAddCallback(
            PartakerInvController invController,
            AefSession session,
            IPartakerInvManager invManager,
            OrgEntity org, TaskEntity task, 
            StaffEntity leader, StaffEntity collabrator, StaffEntity mentor, StaffEntity recipient
            );

        private void SetupTask(QuickAddCallback callback)
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.SessionProvider().GetSession())
                { 
                    //创建一个组织
                    var admin = this.Services.CreateAccount();
                    var org = this.Services.CreateOrg(admin);

                    //创建一个任务
                    var leader = this.Services.CreateAccountStaff(org);
                    var task = this.Services.CreateTask(leader);

                    //为任务加入一个协同者
                    var collabrator = this.Services.CreateAccountStaff(org);
                    this.Services.PartakerInvManager().QuickAdd(task, leader, collabrator, PartakerKinds.Collaborator);
                    //为任务加入一个指导者
                    var mentor = this.Services.CreateAccountStaff(org);
                    this.Services.PartakerInvManager().QuickAdd(task, leader, mentor, PartakerKinds.Mentor);
                    //为任务加入一个接收者
                    var recipient = this.Services.CreateAccountStaff(org);
                    this.Services.PartakerManager().ChangePartakerKind(task, recipient,PartakerKinds.Recipient);

                    PartakerInvController invController = new PartakerInvController(
                        this.Services.SessionProvider(), 
                        this.Services.TaskManager(), 
                        this.Services.StaffManager(),
                        this.Services.PartakerInvManager(),
                        this.Services.NotificationManager(),
                        null,
                        this.Services.IMService());

                    //邀请成员
                    callback(invController, session, this.Services.PartakerInvManager(), org, task, 
                        leader, collabrator, mentor, recipient);

                    session.SaveChanges();
                }

                tx.NoComplete();
            }
        }
    }
}
