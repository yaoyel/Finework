using System;
using FineWork.Colla;
using FineWork.Data.Aef;
using FineWork.Security.Passwords;
using FineWork.Security.Repos.Aef;

namespace FineWork.SysSetup
{
    public class DbMiniSeed : DbSeed
    {
        public DbMiniSeed(FineWorkDbContext dbContext)
            : base(dbContext)
        {
        }

        public override void Seed()
        {
            this.Generate(this.DbContext);
        }

        public void Generate(FineWorkDbContext dbContext)
        {
            Console.WriteLine();

            var thdOrg = CreateOrg(dbContext, "桃花岛", false);
            var thdAdminStaff = CreateAccountStaff(dbContext, "黄药师", "13701000001", thdOrg);
            UpdateOrgAdmin(dbContext, thdOrg, thdAdminStaff);

            CreateAccountStaff(dbContext, "陈玄风", "13701000002", thdOrg);
            CreateAccountStaff(dbContext, "梅超风", "13701000003", thdOrg);
            CreateAccountStaff(dbContext, "曲灵风", "13701000004", thdOrg);
            CreateAccountStaff(dbContext, "陆乘风", "13701000005", thdOrg);
            CreateAccountStaff(dbContext, "武眠风", "13701000006", thdOrg);
            CreateAccountStaff(dbContext, "冯默风", "13701000007", thdOrg);
            CreateAccountStaff(dbContext, "黄蓉", "13701000008", thdOrg);

            var qzjOrg = CreateOrg(dbContext, "全真教", true);
            var qzjAdminStaff = CreateAccountStaff(dbContext, "王重阳", "13702000001", qzjOrg);
            UpdateOrgAdmin(dbContext, qzjOrg, qzjAdminStaff);

            CreateAccountStaff(dbContext, "周伯通", "13702000002", qzjOrg, "周伯通");
            CreateAccountStaff(dbContext, "马钰", "13702000003", qzjOrg, "丹阳子");
            CreateAccountStaff(dbContext, "丘处机", "13702000004", qzjOrg, "长春子");
            CreateAccountStaff(dbContext, "谭处端", "13702000005", qzjOrg, "长真子");
            CreateAccountStaff(dbContext, "王处一", "13702000006", qzjOrg, "玉阳子");
            CreateAccountStaff(dbContext, "郝大通", "13702000007", qzjOrg, "太古子");
            CreateAccountStaff(dbContext, "刘处玄", "13702000008", qzjOrg, "长生子");
            CreateAccountStaff(dbContext, "孙不二", "13702000009", qzjOrg, "清静散人");

            CreateAccount(dbContext, "欧阳峰", "13703000001");
            CreateAccount(dbContext, "段智兴", "13704000001");
            CreateAccount(dbContext, "洪七公", "13705000001");
            CreateAccount(dbContext, "郭靖", "13706000001");
        }

        private static StaffEntity CreateAccountStaff(FineWorkDbContext dbContext, String name, String phoneNumber, OrgEntity org, String staffName = null)
        {
            var account = CreateAccount(dbContext, name, phoneNumber);
            var staff = AddStaff(dbContext, org, account, staffName ?? name);
            return staff;
        }

        private static AccountEntity CreateAccount(FineWorkDbContext dbContext, String name, String phoneNumber)
        {
            AccountEntity account = new AccountEntity();
            account.Id = Guid.NewGuid();
            account.Name = name;
            account.PhoneNumber = phoneNumber;
            account.Email = $"{phoneNumber}@example.com";
            account.Password = "123456";
            account.PasswordFormat = PasswordFormats.ClearText;
            account.CreatedAt = DateTime.Now;

            dbContext.Set<AccountEntity>().Add(account);
            dbContext.SaveChanges();

            Console.WriteLine($"Account {account.Name} created.");
            return account;
        }

        private static OrgEntity CreateOrg(FineWorkDbContext dbContext, String name, bool isInvEnabled)
        {
            OrgEntity org = new OrgEntity();
            org.Id = Guid.NewGuid();
            org.Name = name;
            org.IsInvEnabled = isInvEnabled;
            org.CreatedAt = DateTime.Now;

            dbContext.Set<OrgEntity>().Add(org);
            dbContext.SaveChanges();

            Console.WriteLine($"Org {org.Name} created.");
            return org;
        }

        public static StaffEntity AddStaff(FineWorkDbContext dbContext, OrgEntity org, AccountEntity account, String staffName)
        {
            StaffEntity staff = new StaffEntity();
            staff.Id = Guid.NewGuid();
            staff.Account = account;
            staff.Org = org;
            staff.CreatedAt = DateTime.Now;
            staff.Name = staffName;

            dbContext.Set<StaffEntity>().Add(staff);
            dbContext.SaveChanges();
            return staff;
        }

        private static void UpdateOrgAdmin(FineWorkDbContext dbContext, OrgEntity org, StaffEntity staff)
        {
            org.AdminStaff = staff;
            dbContext.SaveChanges();
        }
    }
}