using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Transactions;
using FineWork.Colla.Models;
using FineWork.Core;
using FineWork.Security;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Colla
{
    [TestFixture]
    public class TaskIncentiveManagerTests: FineWorkCoreTestBase
    {
        [Test]
        public void GetTaskIncentives_equal_incentivekinds()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var incentiveKindManager = this.Services.GetRequiredService<IIncentiveKindManager>();

                    var taskManager = this.Services.GetRequiredService<ITaskManager>();

                    var taskIncentiveManager = this.Services.GetRequiredService<ITaskIncentiveManager>();

                    var accountManager = this.Services.GetRequiredService<IAccountManager>();
                    var orgManager = this.Services.GetRequiredService<IOrgManager>();
                    var staffManager = this.Services.GetRequiredService<IStaffManager>();
                    //创建一个账号
                    var accountName = "Test-Account-Incentive-0001";
                    var account = accountManager.CreateAccount(accountName, "123456", $"{accountName}@example.com",
                        "13701472527");
                    session.SaveChanges();
                    //创建一个组织
                    var org = orgManager.CreateOrg(account, "Test-Org-Incentive-0001", null);
                    session.SaveChanges();
                    //添加一个员工
                    var staff = staffManager.CreateStaff(org.Id, account.Id, account.Name);
                    session.SaveChanges();

                    //创建一个任务
                    var taskModel = new CreateTaskModel();
                    taskModel.Name = "Test-Task-Incentives-0001";
                    taskModel.CreatorStaffId = staff.Id;

                    var task = taskManager.CreateTask(taskModel);
                    session.SaveChanges();

                    var kinds = incentiveKindManager.FetchIncentiveKind();
                    var taskIncentives =
                        taskIncentiveManager.FetchTaskIncentiveByTaskId(task.Id);

                    Assert.AreEqual(kinds.Count(), taskIncentives.Count());

                }
                tx.NoComplete();
            }
        }

        [Test]
        public void GetTaskIncentive_by_kindid_return_cannot_be_null()
        {
            //未设置激励的任务
            var taskId = Guid.NewGuid();
            using (var session = this.Services.ResolveSessionProvider().GetSession())
            {
                var taskIncentiveManager = this.Services.GetRequiredService<ITaskIncentiveManager>();
                var taskIncentive = taskIncentiveManager.FindTaskIncentiveByTaskIdAndKindId(taskId, 1);

                Assert.NotNull(taskIncentive);
                Assert.AreEqual(taskIncentive.Amount, 0); 
            } 
        }
    }
}
