using System;
using System.Collections.Generic;

namespace AppBoot.Shop.NoCast
{
    public class Customer
    {
        public virtual int Id { get; set; }

        public virtual String Name { get; set; }

        private ICollection<Order> m_Orders;

        public virtual ICollection<Order> Orders
        {
            get { return m_Orders ?? (m_Orders = new HashSet<Order>()); }
            set { m_Orders = value; }
        }
    }
}