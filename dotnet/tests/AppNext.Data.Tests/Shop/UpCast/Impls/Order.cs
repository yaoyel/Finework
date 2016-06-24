using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Repos.Core;

namespace AppBoot.Shop.UpCast.Impls
{
    public class Order : IOrder
    {
        public virtual int Id { get; set; }

        #region Customer

        public virtual Customer Customer { get; set; }

        [NotMapped]
        ICustomer IOrder.Customer
        {
            get { return this.Customer; }
            set { this.Customer = (Customer) value; }
        }

        #endregion

        #region OrderItems

        private readonly UpCastSet<OrderItem, IOrderItem> m_OrderItems = new UpCastSet<OrderItem, IOrderItem>();

        /// <summary> Gets or sets the collection of OrderItems. </summary>
        public virtual ICollection<OrderItem> OrderItems
        {
            get { return m_OrderItems; }
            set { m_OrderItems.ResetItems(value); }
        }

        [NotMapped]
        ICollection<IOrderItem> IOrder.OrderItems
        {
            get { return m_OrderItems; }
        }

        #endregion
    }
}