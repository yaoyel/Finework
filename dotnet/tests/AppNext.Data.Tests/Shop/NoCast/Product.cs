using System;
using System.Collections.Generic;

namespace AppBoot.Shop.NoCast
{
    public class Product
    {
        public virtual int Id { get; set; }

        public String Name { get; set; }

        private ICollection<OrderItem> m_OrderItems;

        public virtual ICollection<OrderItem> OrderItems
        {
            get { return m_OrderItems ?? (m_OrderItems = new HashSet<OrderItem>()); }
            set { m_OrderItems = value; }
        }
    }
}