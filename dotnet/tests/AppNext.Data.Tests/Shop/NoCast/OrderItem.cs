namespace AppBoot.Shop.NoCast
{
    public class OrderItem
    {
        public virtual int Id { get; set; }

        public virtual Order Order { get; set; }

        public virtual Product Product { get; set; }
    }
}