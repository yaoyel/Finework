using AppBoot.Repos.Adapters;

namespace AppBoot.Repos.Aef
{
    /// <summary>
    ///  Represents an up-castable repository based on Entity Framework.
    /// </summary>
    /// <typeparam name="TDecl"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    public class AefUpCastRepository<TDecl, TKey, TImpl>
        : UpCastRepositoryAdapter<TDecl, TKey, TImpl, AefRepository<TImpl, TKey>>
        where TDecl : class
        where TImpl : class, TDecl
    {
        public AefUpCastRepository(AefRepository<TImpl, TKey> adaptedRepository) 
            : base(adaptedRepository)
        {
        }

        public AefUpCastRepository(ISessionProvider<AefSession> session)
            : base(new AefRepository<TImpl, TKey>(session))
        {
        }
    }
}