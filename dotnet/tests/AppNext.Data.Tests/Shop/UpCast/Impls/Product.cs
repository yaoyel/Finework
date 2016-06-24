using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using AppBoot.Repos.Core;

namespace AppBoot.Shop.UpCast.Impls
{
    public class Product : IProduct
    {
        public virtual int Id { get; set; }

        public virtual String Name { get; set; }

        #region OrderItems

        private readonly UpCastSet<OrderItem, IOrderItem> m_OrderItems = new UpCastSet<OrderItem, IOrderItem>();

        /// <summary> Gets or sets the collection of OrderItems. </summary>
        public virtual ICollection<OrderItem> OrderItems
        {
            get { return m_OrderItems; }
            set { m_OrderItems.ResetItems(value); }
        }

        [NotMapped]
        ICollection<IOrderItem> IProduct.OrderItems
        {
            get { return m_OrderItems; }
        }

        #endregion
    }
}