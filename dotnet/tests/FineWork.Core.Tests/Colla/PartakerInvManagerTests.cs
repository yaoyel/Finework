using System;
using AppBoot.Transactions;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;
using FineWork.Security;
using FineWork.Security.Models;
using FineWork.Security.Repos.Aef;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Colla
{
    [TestFixture]
    public class PartakerInvManagerTests : FineWorkCoreTestBase
    {
        /// <summary> <see cref="IPartakerInvManager.QuickAdd"/> 可用于邀请指导者. </summary>
        [Test]
        public void QuickAdd_returns_Partaker_for_mentor()
        {
            ForQuickAdd((invManager, org, task, inviter, invitee) =>
            {
                var inv = invManager.QuickAdd(task, inviter, invitee, PartakerKinds.Mentor);

                Assert.IsTrue(inv.InviterStaffIds == inviter.Id.ToString());
                Assert.IsTrue(inv.Staff == invitee);
                Assert.IsTrue(inv.PartakerKind == PartakerKinds.Mentor);
                Assert.IsTrue(inv.ReviewStatus == ReviewStatuses.Approved);
            });
        }

        /// <summary> <see cref="IPartakerInvManager.QuickAdd"/> 可用于邀请协同者. </summary>
        [Test]
        public void QuickAdd_returns_Partaker_for_Collabrator()
        {
            ForQuickAdd((invManager, org, task, inviter, invitee) =>
            {
                var inv = invManager.QuickAdd(task, inviter, invitee, PartakerKinds.Collaborator);

                Assert.IsTrue(inv.InviterStaffIds == inviter.Id.ToString());
                Assert.IsTrue(inv.Staff == invitee);
                Assert.IsTrue(inv.PartakerKind == PartakerKinds.Collaborator);
                Assert.IsTrue(inv.ReviewStatus == ReviewStatuses.Approved);
            });
        }

        /// <summary> 邀请任务参与者时被邀请人的角色不能为 <see cref="PartakerKinds.Unspecified"/>. </summary>
        [Test]
        public void QuickAdd_throws_when_PartakerKind_is_Unspecified()
        {
            ForQuickAdd((invManager, org, task, inviter, invitee) =>
            {
                Assert.Throws<FineWorkException>(() =>
                    invManager.QuickAdd(task, inviter, invitee, PartakerKinds.Unspecified)
                    );
            });
        }

        /// <summary> <see cref="IPartakerInvManager.QuickAdd"/> 不可用于邀请负责人. </summary>
        [Test]
        public void QuickAdd_throws_when_PartakerKind_is_Leader()
        {
            ForQuickAdd((invManager, org, task, inviter, invitee) =>
            {
                Assert.Throws<FineWorkException>(() =>
                    invManager.QuickAdd(task, inviter, invitee, PartakerKinds.Leader)
                    );
            });
        }

        /// <summary> <see cref="IPartakerInvManager.QuickAdd"/> 不可用于邀请接收者. </summary>
        [Test]
        public void QuickAdd_throws_when_PartakerKind_is_Recipient()
        {
            ForQuickAdd((invManager, org, task, inviter, invitee) =>
            {
                Assert.Throws<FineWorkException>(() =>
                    invManager.QuickAdd(task, inviter, invitee, PartakerKinds.Recipient)
                    );
            });
        }

        private delegate void QuickAddCallback(IPartakerInvManager invManager, OrgEntity org, TaskEntity task, StaffEntity inviter, StaffEntity invitee);

        private void ForQuickAdd(QuickAddCallback callback)
        {
            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            var orgManager = this.Services.GetRequiredService<IOrgManager>();
            var staffManager = this.Services.GetRequiredService<IStaffManager>();
            var taskManager = this.Services.GetRequiredService<ITaskManager>();
            var partakerManager = this.Services.GetRequiredService<IPartakerManager>();
            var invManager = this.Services.GetRequiredService<IPartakerInvManager>();

            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    //创建任务负责人（兼邀请人）的账号
                    var inviterName = "Test-Account-001";
                    var inviterAccount = accountManager.CreateAccount(inviterName, inviterName,
                        $"{inviterName}@example.com", FakePhoneNumber);

                    //创建机构
                    var org = orgManager.CreateOrg(inviterAccount, "Test-Org-001", null);
                    session.SaveChanges();

                    //创建:任务的负责人（兼邀请人）对应的员工
                    var inviterStaff = staffManager.CreateStaff(org.Id, inviterAccount.Id, inviterAccount.Name);
                    session.SaveChanges();

                    orgManager.ChangeOrgAdmin(org, inviterStaff.Id);
                    session.SaveChanges();

                    //创建:被邀请的账号与员工
                    var inviteeName = "Test-Account-002";
                    var inviteeAccount = accountManager.CreateAccount(inviteeName, inviteeName,
                        $"{inviteeName}@example.com", FakePhoneNumber);
                    var inviteeStaff = staffManager.CreateStaff(org.Id, inviteeAccount.Id, inviteeAccount.Name);

                    session.SaveChanges(); 
                    //创建任务
                    var taskCreationModel = new CreateTaskModel();
                    taskCreationModel.Name = "Test-Task-001";
                    taskCreationModel.Goal = taskCreationModel.Name;
                    taskCreationModel.CreatorStaffId = inviterStaff.Id; 
                    var task = taskManager.CreateTask(taskCreationModel); 
                    session.SaveChanges();

                    //邀请成员
                    callback(invManager, org, task, inviterStaff, inviteeStaff);
                    session.SaveChanges();
                }

                tx.NoComplete();
            }
        }
    }
}