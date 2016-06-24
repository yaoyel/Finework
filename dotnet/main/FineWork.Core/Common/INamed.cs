using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Files;
using Microsoft.Extensions.DependencyInjection; 

namespace FineWork.Common
{
    /// <summary> 用于在 IoC 容器中通过名称标识一个服务 </summary>
    /// <remarks> 可以通过 <see cref="NamedServiceExtensions.GetRequiredNamedService{T}"/> 获取相应的服务. </remarks>
    public interface INamedService
    {
        /// <summary> 获取服务的名称. </summary>
        String Name { get; }
    }

    public static class NamedServiceExtensions
    {
        private static T[] GetServicesForName<T>(this IServiceProvider serviceProvider, String name)
            where T : INamedService
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Value is null or empty.", nameof(name));

            var factories = serviceProvider.GetServices<T>();
            return factories.Where(factory => factory.Name == name).ToArray();
        }

        public static T GetNamedService<T>(this IServiceProvider serviceProvider, String name) where T : INamedService
        {
            var services = GetServicesForName<T>(serviceProvider, name);

            if (services.Length > 1) throw MultipleServicesRegisteredError<T>(name);
            if (services.Length == 0) return default(T);
            return services[0];
        }

        public static T GetRequiredNamedService<T>(this IServiceProvider serviceProvider, String name) where T : INamedService
        {
            var services = GetServicesForName<T>(serviceProvider, name);

            if (services.Length == 0) throw NoSuchServiceError<T>(name);
            if (services.Length > 1) throw MultipleServicesRegisteredError<T>(name);
            return services[0];
        }

        private static InvalidOperationException NoSuchServiceError<T>(String name)
        {
            var typeName = typeof(T).FullName;
            return new InvalidOperationException($"No {typeName} registered with the name [{name}].");
        }

        private static InvalidOperationException MultipleServicesRegisteredError<T>(String name)
        {
            var typeName = typeof(T).FullName;
            return new InvalidOperationException($"Multiple {typeName} services registered with the name [{name}].");
        }
    }
}
