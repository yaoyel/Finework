using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using AppBoot.Shop.UpCast.Impls;

namespace AppBoot.Repos.Aef.DbRequired.Shop.UpCast
{
    /// <summary> Represents the <see cref="DbContext"/> for entities 
    /// in the namespace <see cref="AppBoot.Shop.UpCast"/>. </summary>
    public class UpCastContext : AefSession
    {
        public UpCastContext()
            : this(false)
        {
        }

        public UpCastContext(bool saveImmediately)
            : base("AppBoot", saveImmediately)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<StoreGeneratedIdentityKeyConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.HasDefaultSchema("UpCast");

            var customers = modelBuilder.Entity<Customer>().HasKey(customer => customer.Id);
            customers.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            var orders = modelBuilder.Entity<Order>().HasKey(order => order.Id);
            orders.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            var products = modelBuilder.Entity<Product>().HasKey(product => product.Id);
            products.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            var orderItems = modelBuilder.Entity<OrderItem>().HasKey(orderItem => orderItem.Id);
            orderItems.Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            customers.HasMany(customer => customer.Orders).WithRequired(order => order.Customer);
            orders.HasMany(order => order.OrderItems).WithRequired(orderItem => orderItem.Order);
            products.HasMany(product => product.OrderItems).WithRequired(orderItem => orderItem.Product);
        }
    }
}