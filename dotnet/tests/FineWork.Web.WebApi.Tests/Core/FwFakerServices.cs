using System;
using System.Collections.Generic;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Core;
using FineWork.Security;

namespace FineWork.Web.WebApi.Tests.Core
{
    /// <summary>
    /// 定义了一组用于单元测试的方法
    /// </summary>
    public class FwFakerServices : FwWrappedServices
    {
        private const String m_FakePhoneNumber = "11111111111";

        public FwFakerServices(IServiceProvider services)
            :base(services)
        {
        }

        public IAccount CreateAccount()
        {
            var accountName = $"Account-{this.NextAccountNumber}";
            var account = this.AccountManager()
                .CreateAccount(accountName, accountName, $"{accountName}@example.com", m_FakePhoneNumber);
            return account;
        }

        public OrgEntity CreateOrg(IAccount account)
        {
            var org = this.OrgManager().CreateOrg(account, $"Org-{this.NextOrgNumber}", null);
            var orgAdminStaff = this.StaffManager().CreateStaff(org.Id, account.Id, account.Name);
            this.OrgManager().ChangeOrgAdmin(org, orgAdminStaff.Id);
            return org;
        }

        public StaffEntity CreateAccountStaff(OrgEntity org)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));

            var account = CreateAccount();
            var staff = this.StaffManager().CreateStaff(org.Id, account.Id, account.Name);
            return staff;
        }

        public TaskEntity CreateTask(StaffEntity creator)
        {
            CreateTaskModel model = new CreateTaskModel();
            model.Name = $"Task-{this.NextTaskNumber}";
            model.CreatorStaffId = creator.Id;
            model.Goal = model.Name;

            var task = this.TaskManager().CreateTask(model);
            return task;
        }

        private int NextAccountNumber
        {
            get { return GetNextNumber("Account"); }
        }

        private int NextOrgNumber
        {
            get { return GetNextNumber("Org"); }
        }

        private int NextTaskNumber
        {
            get { return GetNextNumber("Task"); }
        }

        private Dictionary<string, int> m_NextNumbers = new Dictionary<string, int>();

        private int GetNextNumber(String key)
        {
            int value;
            if (m_NextNumbers.TryGetValue(key, out value))
            {
                m_NextNumbers[key] = ++value;
            }
            else
            {
                value = 1;
                m_NextNumbers[key] = ++value;
            }
            return value;
        }
    }
}