namespace AppBoot.Repos.Adapters
{
    public class Base
    {
        public int Id { get; set; }
    }

    public class Derived : Base
    {
    }

    public interface IDerivedSyncRepository : IRepository<Derived, int>
    {
    }
}