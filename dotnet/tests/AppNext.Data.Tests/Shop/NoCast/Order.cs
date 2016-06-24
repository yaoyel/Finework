using System.Collections.Generic;

namespace AppBoot.Shop.NoCast
{
    public class Order
    {
        public virtual int Id { get; set; }
        
        public virtual Customer Customer { get; set; }

        private ICollection<OrderItem> m_OrderItems;

        /// <summary> Gets or sets the collection of OrderItems. </summary>
        public virtual ICollection<OrderItem> OrderItems
        {
            get { return m_OrderItems ?? (m_OrderItems = new HashSet<OrderItem>()); }
            set { m_OrderItems = value; }
        }
    }
}