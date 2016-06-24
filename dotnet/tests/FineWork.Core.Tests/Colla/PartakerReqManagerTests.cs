using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;
using FineWork.Security;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Colla
{
    [TestFixture]
    public class PartakerReqManagerTests : FineWorkCoreTestBase
    {
        /// <summary> 当 <see cref="TaskEntity.IsCollabratorReqEnabled"/> 为 <c>true</c> 时才能申请成为协同者. </summary>
        [Test]
        public void CreatePartakerReq_creates_PartakerReq_when_IsCollabratorReqEnabled_is_true()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                task.RecruitmentRoles=string.Concat(task.RecruitmentRoles, ',', ((int) PartakerKinds.Collaborator));
                sessionScope.SaveChanges();

                var req = reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Collaborator);
                sessionScope.SaveChanges();

                Assert.AreEqual(ReviewStatuses.Unspecified, req.ReviewStatus);
            });
        }

        /// <summary> 当 <see cref="TaskEntity.IsCollabratorReqEnabled"/> 为 <c>false</c> 时不能申请成为协同者. </summary>
        [Test]
        public void CreatePartakerReq_throws_when_IsCollabratorReqEnabled_is_false()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                task.RecruitmentRoles = "";
                sessionScope.SaveChanges();

                Assert.Throws<FineWorkException>(() =>
                    reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Collaborator)
                    );
            });
        }

        /// <summary> 当 <see cref="TaskEntity.IsMentorReqEnabled"/> 为 <c>true</c> 时才能申请成为指导者. </summary>
        [Test]
        public void CreatePartakerReq_creates_PartakerReq_when_IsMentorReqEnabled_is_true()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                task.RecruitmentRoles = string.Concat(task.RecruitmentRoles, ',', ((int)PartakerKinds.Mentor));

                sessionScope.SaveChanges();

                var req = reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Mentor);
                sessionScope.SaveChanges();

                Assert.AreEqual(ReviewStatuses.Unspecified, req.ReviewStatus);
            });
        }

        /// <summary> 当 <see cref="TaskEntity.IsMentorReqEnabled"/> 为 <c>false</c> 时不能申请成为指导者. </summary>
        [Test]
        public void CreatePartakerReq_throws_when_IsMentorReqEnabled_is_false()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                task.RecruitmentRoles = "";

                sessionScope.SaveChanges();

                Assert.Throws<FineWorkException>(() =>
                    reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Mentor)
                    );
            });
        }

        /// <summary> 任务的接收者只能由任务的负责人指定，而不能申请. </summary>
        [Test]
        public void CreatePartakerReq_throws_when_PartakerKind_is_Recipient()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                Assert.Throws<FineWorkException>(() =>
                    reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Recipient)
                    );
            });
        }

        /// <summary> 申请的角色不能为 <see cref="PartakerKinds.Unspecified"/>. </summary>
        [Test]
        public void CreatePartakerReq_throws_when_PartakerKind_is_Unspecified()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                Assert.Throws<FineWorkException>(() =>
                    reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Unspecified)
                    );
            });
        }

        [Test]
        public void ReviewPartakerReq_returns_reviewed_PartakerReq()
        {
            ForPartakerReq((sessionScope, reqManager, task, requestor) =>
            {
                task.RecruitmentRoles = string.Concat(task.RecruitmentRoles, ',', ((int)PartakerKinds.Collaborator));

                sessionScope.SaveChanges();

                var req = reqManager.CreatePartakerReq(task, requestor, PartakerKinds.Collaborator);
                sessionScope.SaveChanges();

                reqManager.ReviewPartakerReq(req, ReviewStatuses.Approved);
                Assert.AreEqual(ReviewStatuses.Approved, req.ReviewStatus);
            });
        }

        private delegate void PartakerReqAction(AefSession session,
            IPartakerReqManager reqManager, TaskEntity task, StaffEntity requestor);

        private void ForPartakerReq(PartakerReqAction callback)
        {
            var sf = this.Services.GetRequiredService<ISessionProvider<AefSession>>();

            var accountManager = this.Services.GetRequiredService<IAccountManager>();
            var orgManager = this.Services.GetRequiredService<IOrgManager>();
            var staffManager = this.Services.GetRequiredService<IStaffManager>();
            var taskManager = this.Services.GetRequiredService<ITaskManager>();
            var partakerManager = this.Services.GetRequiredService<IPartakerManager>();
            var reqManager = this.Services.GetRequiredService<IPartakerReqManager>();

            using (var tx = TxManager.Acquire())
            {
                var session = sf.GetSession();

                //创建任务负责人（兼审批人）的账号
                var reviewerName = "Test-Account-001";
                var reviewerAccount = accountManager.CreateAccount(reviewerName, reviewerName,
                    $"{reviewerName}@example.com", FakePhoneNumber);

                //创建机构
                var org = orgManager.CreateOrg(reviewerAccount, "Test-Org-001", null);
                session.SaveChanges();

                //创建:任务的负责人（兼审批人）对应的员工
                var inviterStaff = staffManager.CreateStaff(org.Id, reviewerAccount.Id, reviewerAccount.Name);
                session.SaveChanges();

                orgManager.ChangeOrgAdmin(org, inviterStaff.Id);
                session.SaveChanges();

                //创建:申请人的账号与员工
                var requestorName = "Test-Account-002";
                var requestorAccount = accountManager.CreateAccount(requestorName, requestorName,
                    $"{requestorName}@example.com", FakePhoneNumber);
                var requestorStaff = staffManager.CreateStaff(org.Id, requestorAccount.Id, requestorAccount.Name);

                session.SaveChanges();

                //创建任务
                var taskCreationModel = new CreateTaskModel();
                taskCreationModel.Name = "Test-Task-001";
                taskCreationModel.Goal = taskCreationModel.Name;
                taskCreationModel.CreatorStaffId = inviterStaff.Id;

                var task = taskManager.CreateTask(taskCreationModel);
                session.SaveChanges();

                //创建申请
                callback(session, reqManager, task, requestorStaff);

                session.SaveChanges();


                tx.NoComplete();
            }
        }
    }
}
