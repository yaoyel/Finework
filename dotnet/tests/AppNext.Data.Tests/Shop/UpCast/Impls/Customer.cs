using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Repos.Core;

namespace AppBoot.Shop.UpCast.Impls
{
    public class Customer : ICustomer
    {
        public virtual int Id { get; set; }

        public virtual String Name { get; set; }

        #region Orders

        private readonly UpCastSet<Order, IOrder> m_Orders = new UpCastSet<Order, IOrder>();

        /// <summary> Entity implementation collection of Order. </summary>
        public virtual ICollection<Order> Orders
        {
            get { return m_Orders; }
            set { m_Orders.ResetItems(value); }
        }
        
        [NotMapped]
        ICollection<IOrder> ICustomer.Orders
        {
            get { return m_Orders; }
        }

        #endregion
    }
}