using System;
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
    public class TaskAnncManagerTests: FineWorkCoreTestBase
    {
        [Test]
        public void CreateAnnc_create_and_review_annc()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var accountManger = this.Services.GetRequiredService<IAccountManager>();
                    var orgManager = this.Services.GetRequiredService<IOrgManager>();
                    var staffManager = this.Services.GetRequiredService<IStaffManager>();
                    var taskManager = this.Services.GetRequiredService<ITaskManager>();
                    var partakerManager = this.Services.GetRequiredService<IPartakerManager>();
                    var taskSharingManager = this.Services.GetRequiredService<ITaskSharingManager>();

                    var incentiveKindManager = this.Services.GetRequiredService<IIncentiveKindManager>();
                    var anncManager = this.Services.GetRequiredService<IAnnouncementManager>();
                    var anncAttManager = this.Services.GetRequiredService<IAnncAttManager>();
                    var anncIncentiveManager = this.Services.GetRequiredService<IAnncIncentiveManager>();

                    var account = accountManger.CreateAccount("test-account-001", "123456", "", FakePhoneNumber);
                    var account2 = accountManger.CreateAccount("test-account-002", "123456", "", "13709090909");
                    session.SaveChanges();

                    var org = orgManager.CreateOrg(account, "testorg-001", null);
                    session.SaveChanges();

                    var staff = staffManager.CreateStaff(org.Id, account.Id, "test-staff-001");
                    var staff2 = staffManager.CreateStaff(org.Id, account2.Id, "test-staff-002");
                    session.SaveChanges();

                    var taskModel=new CreateTaskModel();
                    taskModel.Name = "test-test-001";
                    taskModel.EndAt = DateTime.Now.AddMonths(2);
                    taskModel.CreatorStaffId = staff.Id;
                    var task = taskManager.CreateTask(taskModel);
                    session.SaveChanges();


                    var createAnncModel=new CreateAnncModel();
                    createAnncModel.Content = "AnncContent";
                    createAnncModel.EndAt=DateTime.Now;
                    createAnncModel.IsNeedAchv = true;
                    createAnncModel.StaffId = staff2.Id;
                    createAnncModel.TaskId = task.Id;
                    var annc=anncManager.CreateAnnc(createAnncModel);

                    Assert.NotNull(annc);
                    Assert.AreEqual(annc.Content,"AnncContent");
                    Assert.True(annc.IsNeedAchv);
                    Assert.AreEqual(annc.Task.Id,task.Id);
                    Assert.AreEqual(annc.Staff.Id,staff2.Id);


                    Assert.Throws<FineWorkException>(() =>
                    {
                        var id = Guid.NewGuid();
                        anncAttManager.CreateAnncAtt(annc.Id, id, true);
                    });

                    Assert.Throws<FineWorkException>(() =>
                    {
                        anncIncentiveManager.CreateOrUpdateAnncIncentive(annc.Id, 1, 100);
                    });


                    tx.NoComplete();
                }
            }
        }
    }
}