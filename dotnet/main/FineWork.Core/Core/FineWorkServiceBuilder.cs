using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace FineWork.Core
{
    /// <summary> <see cref="FineWorkServiceBuilder"/> narrows down 
    /// the extension point for service registration methods. </summary>
    public class FineWorkServiceBuilder
    {
        public FineWorkServiceBuilder([NotNull] IServiceCollection serviceCollection)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            this.ServiceCollection = serviceCollection;
        }

        public IServiceCollection ServiceCollection { get; private set; }
    }
}