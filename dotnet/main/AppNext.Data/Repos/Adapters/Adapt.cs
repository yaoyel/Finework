namespace AppBoot.Repos.Adapters
{
    public static class Adapt
    {
        public static class Up<T, TKey, TImpl>
            where T : class
            where TImpl : class, T
        {
            public static UpCastRepositoryAdapter<T, TKey, TImpl, TRepository> From<TRepository>(
                TRepository repository)
                where TRepository : IRepository<TImpl, TKey>
            {
                return new UpCastRepositoryAdapter<T, TKey, TImpl, TRepository>(repository);
            }
        }
    }
}