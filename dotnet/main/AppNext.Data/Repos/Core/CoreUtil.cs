using System;
using AppBoot.Repos.Exceptions;

namespace AppBoot.Repos.Core
{
    public static class CoreUtil
    {
        public static T CheckedArg<T>(T arg, String argName) where T : class
        {
            if (arg == null) throw new ArgumentNullException(argName);
            return arg;
        }

        public static TImpl CastEntity<TDecl, TImpl>(this IRepository<TImpl> repository, TDecl entity)
            where TDecl : class
            where TImpl : class, TDecl
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (entity == null) throw new ArgumentNullException("entity");

            try
            {
                var result = (TImpl) entity;
                return result;
            }
            catch (InvalidCastException e)
            {
                throw new InvalidEntityTypeException(typeof (TImpl), entity.GetType(), e);
            }
        }
    }
}
