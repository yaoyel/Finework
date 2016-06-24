using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Data.Aef;
using FineWork.Security.Passwords;
using FineWork.Security.Repos.Aef;

namespace FineWork.SysSetup
{
    public class DbBulkSeed : DbSeed
    {
        public DbBulkSeed(FineWorkDbContext dbContext)
            : base(dbContext)
        {
        }

        public int NumberOfAccounts { get; set; } = 100;

        public int NumberOfOrgs { get; set; } = 10;

        public int NumberOfTasks { get; set; } = 1000;

        public IDictionary<string, string> IncentiveKind { get; set; } = new Dictionary<string, string>()
        {
            ["奖金值"] = "元",
            ["能力值"] = "PS",
            ["人品值"] = "RP",
            ["职星值"] = "星",
            ["股票数"] = "股"
        };
        

        public override void Seed()
        {
            var random = new Random();
            const int lineWidth = 80;

            var newAccounts = new List<AccountEntity>();
            for (int accountIndex = 1; accountIndex <= NumberOfAccounts; accountIndex++)
            {
                Console.Write($"\rGenerating {NumberOfAccounts} Accounts .... {accountIndex}");

                var account = CreateNewAccount(accountIndex);

                newAccounts.Add(account);
                this.Accounts.Add(account);
                this.DbContext.SaveChanges();
            }
            Console.WriteLine($"\rGenerating {NumberOfAccounts} Accounts .... OK".PadRight(lineWidth));

            var newOrgs = new List<OrgEntity>();
            for (int orgIndex = 1; orgIndex <= NumberOfOrgs; orgIndex++)
            {
                Console.Write($"\rGenerating {NumberOfOrgs} Orgs .... {orgIndex}");

                var org = CreateNewOrg(orgIndex);

                newOrgs.Add(org);
                this.Orgs.Add(org);
                this.DbContext.SaveChanges();
            }
            Console.WriteLine($"\rGenerating {NumberOfOrgs} Orgs .... OK".PadRight(lineWidth));

            var numberOfStaffs = NumberOfAccounts;
            var newStaffs = new List<StaffEntity>();
            for (var staffIndex = 1; staffIndex <= numberOfStaffs; staffIndex ++)
            {
                Console.Write($"\rGenerating {numberOfStaffs} Staffs .... {staffIndex}");

                var account = newAccounts[staffIndex - 1];
                var org = newOrgs[random.Next(newOrgs.Count - 1)];
                var staff = CreateNewStaff(staffIndex, org, account);

                newStaffs.Add(staff);
                this.Staffs.Add(staff);
                this.DbContext.SaveChanges();
            }
            Console.WriteLine($"\rGenerating {numberOfStaffs} Staffs .... OK".PadRight(lineWidth));

            var newTasks = new List<TaskEntity>();
            for (int taskIndex = 1; taskIndex <= NumberOfTasks; taskIndex++)
            {
                Console.Write($"\rGenerating {NumberOfTasks} Tasks .... {taskIndex}");

                var staff = newStaffs[random.Next(newStaffs.Count - 1)];
                var task = CreateNewTask(taskIndex, staff);

                newTasks.Add(task);
                this.Tasks.Add(task);
                this.DbContext.SaveChanges();

                //增加参与者
                var leader = new PartakerEntity();
                leader.Id = Guid.NewGuid();
                leader.Task = task;
                leader.Kind = PartakerKinds.Leader;
                leader.Staff = task.Creator;
                this.Partakers.Add(leader);

                this.DbContext.SaveChanges();
            }
            Console.WriteLine($"\rGenerating {NumberOfTasks} Tasks .... OK".PadRight(lineWidth));

            for (int kindIndex = 1; kindIndex <= IncentiveKind.Count(); kindIndex++)
            {
                Console.Write($"\rGenerating {IncentiveKind.Count()} IncentiveKind .... {kindIndex}");
                var  item= IncentiveKind.Skip(kindIndex - 1).First();
                var kind=CreateNewIncentiveKind(kindIndex, item.Key, item.Value);
                this.IncentiveKinds.Add(kind);
                this.DbContext.SaveChanges(); 
            }
            Console.WriteLine($"\rGenerating {IncentiveKind.Count()} IncentiveKind .... OK".PadRight(lineWidth));
        }

        private AccountEntity CreateNewAccount(int accountIndex)
        {
            AccountEntity account = new AccountEntity();
            account.Id = Guid.NewGuid();
            account.Name = "Account-" + accountIndex;
            account.Email = account.Name + "example.com";
            account.Password = "123456";
            account.PasswordFormat = PasswordFormats.ClearText;
            account.CreatedAt = DateTime.Now;
            return account;
        }

        private OrgEntity CreateNewOrg(int orgIndex)
        {
            OrgEntity org = new OrgEntity();
            org.Id = Guid.NewGuid();
            org.Name = "Org-" + orgIndex;
            return org;
        }

        private StaffEntity CreateNewStaff(int staffIndex, OrgEntity org, AccountEntity account)
        {
            StaffEntity staff = new StaffEntity();
            staff.Id = Guid.NewGuid();
            staff.Account = account;
            staff.Org = org;
            staff.Name = account.Name;
            return staff;
        }

        private TaskEntity CreateNewTask(int taskIndex, StaffEntity staff)
        {
            TaskEntity task = new TaskEntity();
            task.Id = Guid.NewGuid();
            task.Name = "Task-" + taskIndex;
            task.Creator = staff;
            task.Level = 3;
            task.Goal = task.Name;
            return task;
        }

        private IncentiveKindEntity CreateNewIncentiveKind(int id, string name, string unit)
        {
            return new IncentiveKindEntity()
            {
                Id = id,
                Name = name,
                Unit = unit
            }; 
        }
    }
}
