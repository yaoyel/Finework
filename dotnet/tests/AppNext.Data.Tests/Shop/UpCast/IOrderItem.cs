namespace AppBoot.Shop.UpCast
{
    public interface IOrderItem
    {
        int Id { get; }

        IOrder Order { get; set; }

        IProduct Product { get; set; }
    }
}