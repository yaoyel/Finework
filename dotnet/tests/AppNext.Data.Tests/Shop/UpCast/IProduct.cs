using System.Collections.Generic;

namespace AppBoot.Shop.UpCast
{
    public interface IProduct
    {
        int Id { get; }

        ICollection<IOrderItem> OrderItems { get; }
    }
}