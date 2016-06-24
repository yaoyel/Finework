using System;
using Microsoft.Extensions.DependencyInjection;

namespace FineWork.Core
{
    /// <summary> 用于对两个或多个彼此依赖的服务使用 constructor injection. </summary>
    public interface ILazyResolver<T>
    {
        T Required { get; }

        T Optional { get; }
    }

    public class LazyResolver<T> : ILazyResolver<T>
    {
        public LazyResolver(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            this.m_ServiceProvider = serviceProvider;
        }

        private readonly IServiceProvider m_ServiceProvider;

        public T Required
        {
            get { return m_ServiceProvider.GetRequiredService<T>(); }
        }

        public T Optional
        {
            get { return m_ServiceProvider.GetService<T>(); }
        }
    }
}
