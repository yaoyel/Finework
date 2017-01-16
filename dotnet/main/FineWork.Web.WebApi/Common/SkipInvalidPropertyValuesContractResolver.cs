using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FineWork.Web.WebApi.Common
{
    internal class SkipDefaultPropertyValuesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member,
            MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            var memberProp = member as PropertyInfo;
            var memberField = member as FieldInfo;


            property.ShouldSerialize = obj =>
            {
                object value = memberProp != null
                    ? memberProp.GetValue(obj, null)
                    : memberField?.GetValue(obj);

                if (value == null)
                    return false;
                if (value is bool)
                    return (bool) value;
                else if (value is string)
                    return !string.IsNullOrWhiteSpace(value.ToString());
                else if (value is Guid)
                    return (Guid) value != Guid.Empty;
                else if (value is DateTime)
                    return !value.ToString().StartsWith("0001");
                else if (value is IList)
                {
                    return ((IList) value).Count != 0;
                }
                return true;
            };

            return property;
        }
    }

}