using System;
using System.Collections.Generic;

namespace AppBoot.Shop.UpCast
{
    public interface ICustomer
    {
        int Id { get; }

        String Name { get; set; }

        ICollection<IOrder> Orders { get; }
    }
}