using System;
using System.Data.Entity;
using FineWork.Colla;
using FineWork.Data.Aef;
using FineWork.Security.Repos.Aef;

namespace FineWork.SysSetup
{
    public abstract class DbSeed
    {
        public DbSeed(FineWorkDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            this.DbContext = dbContext;

            this.Accounts = dbContext.Set<AccountEntity>();
            this.Orgs = dbContext.Set<OrgEntity>();
            this.Staffs = dbContext.Set<StaffEntity>();
            this.Tasks = dbContext.Set<TaskEntity>();
            this.Partakers = dbContext.Set<PartakerEntity>();
            this.IncentiveKinds = dbContext.Set<IncentiveKindEntity>();
        }

        protected FineWorkDbContext DbContext { get; }

        protected DbSet<AccountEntity> Accounts { get; }
        protected DbSet<OrgEntity> Orgs { get; }
        protected DbSet<StaffEntity> Staffs { get; }
        protected DbSet<TaskEntity> Tasks { get; }
        protected DbSet<PartakerEntity> Partakers { get; } 
        protected DbSet<IncentiveKindEntity> IncentiveKinds { get; }

        public abstract void Seed();
    }
}