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
    public  class TaskAlarmManagerTests: FineWorkCoreTestBase
    {
 
        [Test]
        public void GetTaskAlarms_for_different_partakerkind()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var taskAlarmManager = this.Services.GetRequiredService<ITaskAlarmManager>();
                    var taskMananger = this.Services.GetRequiredService<ITaskManager>();
                    var accountManager = this.Services.GetRequiredService<IAccountManager>();
                    var staffManager = this.Services.GetRequiredService<IStaffManager>();
                    var orgManager = this.Services.GetRequiredService<IOrgManager>();
                    var partakerInvManager = this.Services.GetRequiredService<IPartakerInvManager>();
                    //创建一个账号
                    var accountName = "Test-Account-Alarm-001";
                    var account = accountManager.CreateAccount(accountName, "123456", $"{accountName}@example.com", "13701472527");

                    //创建一个账号(用于协作者)
                    var accountNameForCollaborator = "Test-Account-Alarm-002";
                    var accountForCollaborator = accountManager.CreateAccount(accountNameForCollaborator, "123456", $"{accountNameForCollaborator}@example.com", "13701472528");
                    
                    //创建一个账号(用于管理者)
                    var accountNameForMentor = "Test-Account-Alarm-003";
                    var accountForMentor = accountManager.CreateAccount(accountNameForMentor, "123456", $"{accountNameForMentor}@example.com", "13701472529");

                    session.SaveChanges();
                    //创建一个组织
                    var org = orgManager.CreateOrg(account, "Test-Org-Alarm-001", null);
                    session.SaveChanges();
                    //添加一个员工
                    var staff = staffManager.CreateStaff(org.Id, account.Id, account.Name); 

                    //添加一个员工用于协作者
                    var staffForCollaborator = staffManager.CreateStaff(org.Id, accountForCollaborator.Id,
                        accountForCollaborator.Name);
                    //添加一个员工用于管理者
                    var staffForMentor = staffManager.CreateStaff(org.Id, accountForMentor.Id, accountForMentor.Name);
                   
                    session.SaveChanges();
                    //创建一个任务
                    var taskModel = new CreateTaskModel();
                    taskModel.Name = "Test-Task-Alarm-001";
                    taskModel.CreatorStaffId = staff.Id;
                    var task = taskMananger.CreateTask(taskModel);
                    //为任务添加一个协作者
                    var collaborator = partakerInvManager.QuickAdd(task, staff, staffForCollaborator,
                        PartakerKinds.Collaborator);

                    //为任务添加一个管理者
                    var mentor = partakerInvManager.QuickAdd(task, staff, staffForMentor, 
                        PartakerKinds.Mentor);

                    session.SaveChanges();
                    var alarlm = new CreateTaskAlarmModel()
                    {
                        TaskId=task.Id,
                        StaffId=staff.Id,
                        AlarmKind= TaskAlarmKinds.PhysicalStrength,
                        Content="类死个人了。。"  
                    };
                    //指导者创建一个预警==》累趴了
                    var alarm = taskAlarmManager.CreateTaskAlarm(alarlm);
                    session.SaveChanges();
                    Assert.NotNull(alarm);

                    //指导者获取预警,该预警应该出现在sendouts（我发送的预警）列表中 
                    var alarmsForCreator = taskAlarmManager.FetchTaskAlarmsByResolvedStatus(task.Id, staff.Id, false);
                    Assert.Contains(alarm, alarmsForCreator.ToList());

                    //预警应该出现在管理者的Receiveds(我收到的预警)列表中
                    var alarmForMentorSatffId = taskAlarmManager.FetchTaskAlarmsByResolvedStatus(task.Id, staffForMentor.Id, false);
                    Assert.Contains(alarm, alarmForMentorSatffId.ToList());


                    //预警应该出现在管理者的others(其他人的预警)列表中
                    var alarmForCollaborator = taskAlarmManager.FetchTaskAlarmsByResolvedStatus(task.Id, staffForCollaborator.Id, false);
                    Assert.Contains(alarm, alarmForCollaborator.ToList());

                }
                tx.NoComplete();
            }
        }
    }
}
