using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Common
{
    public class PageList<T>
    {
        public int? Page { get; set; }

        public int? PageSize { get; set; } 

        public int Total { get; set; }

        public IEnumerable<T> Data { get; set; } 
    }

    public  static  class QueryableUtil
    { 
        public static  PageList<T> ToPagedList<T>(this IQueryable<T>source,int? page,int? pageSize)
        { 
            if (!page.HasValue && !pageSize.HasValue)
                return new PageList<T>() {Page = null,PageSize = null,Total = source.Count(),Data = source.ToList()};

            if (page.HasValue && page.Value <= 0)
                page = null;

            var actualPage = page ?? 1;
            var actualPageSize = pageSize ?? 20;

            if (!page.HasValue && !pageSize.HasValue)
                actualPageSize = source.Count();
          
            var data= source.Skip((actualPage - 1) * actualPageSize).Take(actualPageSize).ToList();
            return new PageList<T>() { Page = page??1, PageSize = pageSize??20, Total = source.Count(), Data = data };
        } 
    }
}
