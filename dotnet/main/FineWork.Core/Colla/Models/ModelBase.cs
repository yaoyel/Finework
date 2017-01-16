using System;
using System.Linq;
using AppBoot.Common;

namespace FineWork.Colla.Models
{
    public class ModelBase
    {

        /// <summary> Creates a new instance. </summary>
        protected ModelBase()
        {
        } 
        public virtual DateTime Time { get; set; }

    }


    public static class ModelExtensions
    {
        public static PageList<T> ToPagedList<T>(this IQueryable<T> source, DateTime? time, int? pageSize) where T:ModelBase
        {
            if (!time.HasValue && !pageSize.HasValue)
                return new PageList<T>() { Page = null, PageSize = null, Total = source.Count(), Data = source.ToList() }; 
         
             pageSize = pageSize ?? 20; 
            time = time ?? DateTime.Now;
            var data = source.Where(p => p.Time <= time);
            return new PageList<T>() { Page =null, PageSize = pageSize ?? 20, Total = source.Count(), Data =data.Take(pageSize.Value)
        };
        }
    }

}