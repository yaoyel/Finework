using System.ComponentModel.DataAnnotations.Schema;

namespace AppBoot.Shop.UpCast.Impls
{
    public class OrderItem : IOrderItem
    {
        public virtual int Id { get; set; }

        #region Order

        public virtual Order Order { get; set; }

        [NotMapped]
        IOrder IOrderItem.Order
        {
            get { return this.Order; }
            set { this.Order = (Order) value; }
        }

        #endregion

        #region Product

        public virtual Product Product { get; set; }

        [NotMapped]
        IProduct IOrderItem.Product
        {
            get { return this.Product; }
            set { this.Product = (Product) value; }
        }

        #endregion
    }
}