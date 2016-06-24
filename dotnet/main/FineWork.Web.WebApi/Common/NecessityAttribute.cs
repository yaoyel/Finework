using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FineWork.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FineWork.Web.WebApi.Common
{
    [AttributeUsage(AttributeTargets.Property)] 
    public class NecessityAttribute:Attribute
    {
        public NecessityLevel Level; 

        public NecessityAttribute(NecessityLevel level =NecessityLevel.High)
        {
            this.Level =  level; 
        }
    }

    public static class NecessityAttributeUitl<T,TV> where T:class,new()
    {
        public static void SetVuleByNecssityAttribute(T model, TV entity,IDictionary<string,Func<TV,dynamic>> dic, bool isShowhighOnly, bool isShowLow)
        {
           
            var modelType = typeof(T);

            var allProperties = modelType.GetProperties().ToList();

            var hasNecessityAttributePro = allProperties.Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(NecessityAttribute)))
                .Select(p => new
                {
                    propertiy = p,
                    level = ((NecessityAttribute)(p.GetCustomAttributes(typeof(NecessityAttribute), false).First())).Level
                }).ToList();

            var neceeityIsHighPros = hasNecessityAttributePro.Where(p => p.level == NecessityLevel.High).ToList();
            var neceeityIsLowPros = hasNecessityAttributePro.Except(neceeityIsHighPros).ToList();

            if (isShowhighOnly && neceeityIsHighPros.Any())
            {
                neceeityIsHighPros.ForEach(p =>
                {
                    p.propertiy.SetValue(model, dic[p.propertiy.Name](entity));
                });

              
                return ;
            }

            if (!isShowLow && neceeityIsLowPros.Any())
            {
                neceeityIsLowPros.ForEach(p =>
                {

                    var targetType = Nullable.GetUnderlyingType(p.propertiy.PropertyType); 

                    var propertyVal = Convert.ChangeType(dic[p.propertiy.Name](entity), targetType);

                    p.propertiy.SetValue(model, propertyVal, null);
                });
                allProperties = allProperties.Except(neceeityIsLowPros.Select(p=>p.propertiy).ToList()).ToList();
            }

            allProperties.ForEach(p =>
                {
                    if(dic.Any(a=>a.Key==p.Name))
                    p.SetValue(model, dic[p.Name](entity));
                }); 
           
        }
    }

}



 