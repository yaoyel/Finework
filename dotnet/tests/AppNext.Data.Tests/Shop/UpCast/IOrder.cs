using System.Collections.Generic;

namespace AppBoot.Shop.UpCast
{
    public interface IOrder
    {
        int Id { get; }

        ICustomer Customer { get; set; }

        ICollection<IOrderItem> OrderItems { get; }
    }
}