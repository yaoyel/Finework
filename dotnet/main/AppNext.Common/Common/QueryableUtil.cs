using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Common
{
    public static  class QueryableUtil
    {
        public static List<T> ToPagedList<T>(this IQueryable<T>source,int? page,int? pageSize)
        {
            var actualPage = page ?? 1;
            var actualPageSize = pageSize ?? 20;

            if (!page.HasValue && !pageSize.HasValue)
                actualPageSize = source.Count();
          
            return source.Skip((actualPage - 1) * actualPageSize).Take(actualPageSize).ToList();
        }
    }
}
