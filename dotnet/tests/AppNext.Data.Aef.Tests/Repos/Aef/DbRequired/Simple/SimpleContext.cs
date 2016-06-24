using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AppBoot.Repos.Aef.DbRequired.Simple
{
    public class SimpleContext : AefSession
    {
        public SimpleContext()
            : this(false)
        {
        }

        public SimpleContext(bool saveImmediately)
            : base("AppBoot", saveImmediately)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<StoreGeneratedIdentityKeyConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.HasDefaultSchema("Simple");

            var simple = modelBuilder.Entity<SimpleEntity>().HasKey(customer => customer.Id);
            simple.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}