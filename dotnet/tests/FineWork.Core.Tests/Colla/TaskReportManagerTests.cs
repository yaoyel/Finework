using System;
using System.Collections.Generic;
using System.IO;
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
    public  class TaskReportManagerTests:FineWorkCoreTestBase
    {
        [Test]
        public void CreateTaskReport_creates_taskreport()
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

                    var taskReportManager = this.Services.GetRequiredService<ITaskReportManager>();
                    var taskReportAttManager = this.Services.GetRequiredService<ITaskReportAttManager>();

                    var account = accountManger.CreateAccount("test-account-01", "123456", "", FakePhoneNumber);
                    var account2 = accountManger.CreateAccount("test-account-02", "123456", "", "13701000002");
                    var account3= accountManger.CreateAccount("test-account-03", "123456", "", "13701000003");
                    var account4 = accountManger.CreateAccount("test-account-04", "123456", "", "13701000004");
                    session.SaveChanges();

                    var org = orgManager.CreateOrg(account, "testorg-001", null);
                    session.SaveChanges();

                    var staff = staffManager.CreateStaff(org.Id, account.Id, "test-staff-001");
                    var staff2 = staffManager.CreateStaff(org.Id, account2.Id, "test-staff-002");
                    var staff3 = staffManager.CreateStaff(org.Id, account3.Id, "test-staff-003");
                    var staff4= staffManager.CreateStaff(org.Id, account4.Id, "test-staff-004");
                    session.SaveChanges();

                    var taskModel=new CreateTaskModel();
                    taskModel.Name = "test-task-001";
                    taskModel.EndAt = DateTime.Now.AddMonths(2);
                    taskModel.CreatorStaffId = staff.Id;
                    var task = taskManager.CreateTask(taskModel);
                    session.SaveChanges();

                    var partaker2 = partakerManager.CreateCollabrator(task.Id, staff2.Id);
                    var partaker3 = partakerManager.CreateCollabrator(task.Id, staff3.Id);
                    var partaker4 = partakerManager.CreateCollabrator(task.Id, staff4.Id);
                    session.SaveChanges();

                    var taskReportModel=new CreateTaskReportModel();
                    taskReportModel.Exilses = new[] {partaker2.Id,partaker3.Id,partaker4.Id};
                    taskReportModel.EffScore = 4;
                    taskReportModel.QualityScore = 5;
                    taskReportModel.EndedAt=DateTime.Now;
                    taskReportModel.Summary = "任务总结";
                    taskReportModel.TaskId = task.Id;
                    var report=taskReportManager.CreateTaskReport(taskReportModel);
                    session.SaveChanges();

                
                }

            }
        }
    }
}
