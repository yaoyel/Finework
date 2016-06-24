using System;
using AppBoot.Repos.Adapters;

namespace AppBoot.Repos.Inmem
{
    public class InmemUpCastRepository<TDecl, TKey, TImpl>
         : UpCastRepositoryAdapter<TDecl, TKey, TImpl, InmemRepository<TImpl, TKey>>
         where TDecl : class
         where TImpl : class, TDecl
    {
        public InmemUpCastRepository(InmemRepository<TImpl, TKey> adaptedRepository)
            : base(adaptedRepository)
        {
        }

        public InmemUpCastRepository(Func<TDecl, TKey> keyGetter)
            : base(new InmemRepository<TImpl, TKey>(keyGetter))
        {
        }
    }
}