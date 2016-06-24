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

namespace FineWork.Colla.Impls
{
    public class MomentManager:AefEntityManager<MomentEntity, Guid>, IMomentManager{
        public MomentManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager ) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager)); 
            m_StaffManager = staffManager; 
        }

        private readonly IStaffManager m_StaffManager; 
        public MomentEntity CreateMement(CreateMomentModel momentModel)
        {
            Args.NotNull(momentModel, nameof(momentModel));

            var staff = StaffExistsResult.Check(m_StaffManager, momentModel.StaffId).ThrowIfFailed().Staff;

            var mement = new MomentEntity()
            {
                Id=Guid.NewGuid(),
                Type= momentModel.Type,
                Content= momentModel.Content,
                Staff=staff
            };

            this.InternalInsert(mement);
            return mement;
        }

        public MomentEntity FindMementById(Guid mementId)
        {
            return this.InternalFind(mementId);
        }

        public IEnumerable<MomentEntity> FetchMementsByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId);
        }

        public IEnumerable<MomentEntity> FetchMementsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public void DeleteMement(Guid mementId)
        {
            var mement = MomentExistsResult.Check(this, mementId).ThrowIfFailed().Moment;
            this.InternalDelete(mement);
        }
    }
}
