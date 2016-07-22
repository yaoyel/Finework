using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Message;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class MomentManager:AefEntityManager<MomentEntity, Guid>, IMomentManager{
        public MomentManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,IConfiguration config,
            INotificationManager nofiNotificationManager) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(nofiNotificationManager, nameof(nofiNotificationManager));  
            Args.NotNull(config, nameof(config));
            m_StaffManager = staffManager;
            m_Config = config;
            m_NotificationManager = nofiNotificationManager;
        }

        private readonly IStaffManager m_StaffManager;

        private readonly IConfiguration m_Config;

        private readonly INotificationManager m_NotificationManager;
        public MomentEntity CreateMoment(CreateMomentModel momentModel)
        {
            Args.NotNull(momentModel, nameof(momentModel));

            var staff = StaffExistsResult.Check(m_StaffManager, momentModel.StaffId).ThrowIfFailed().Staff;

            var moment = new MomentEntity()
            {
                Id=Guid.NewGuid(),
                Type= momentModel.Type,
                Content= momentModel.Content,
                Staff=staff
            };

            this.InternalInsert(moment);

            Task.Factory.StartNew(async () =>
            {
                //给组织下的成员发消息
                foreach (var member in staff.Org.Staffs.ToList())
                {
                    if (member != moment.Staff)
                        await SendMomentMessageAsync(member, moment);
                }
            });
            return moment;
        }

        public MomentEntity FindMomentById(Guid mementId)
        {
            return this.InternalFind(mementId);
        }

        public IEnumerable<MomentEntity> FetchMomentsByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId);
        }

        public IEnumerable<MomentEntity> FetchMomentsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public void DeleteMoment(Guid mementId)
        {
            var mement = MomentExistsResult.Check(this, mementId).ThrowIfFailed().Moment;
            this.InternalDelete(mement);
        }

      
        public async Task SendMomentMessageAsync(StaffEntity staff, MomentEntity moment)
        { 
            string message = string.Format(m_Config["PushMessage:Moment"]);

            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "moment");
            extra.Add("OrgId", moment.Staff.Org.Id.ToString());

            await m_NotificationManager.SendByAliasAsync("", message, extra, staff.Account.PhoneNumber);
        }
    }
}
