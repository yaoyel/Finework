using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FineWork.Core
{
    /// <summary>
    /// <see cref="ServiceScopedTestBase"/> enables each test running in an <see cref="IServiceScope"/>.
    /// </summary>
    public abstract class ServiceScopedTestBase
    {
        [SetUp]
        public virtual void SetUp()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();

            var factory = services.GetRequiredService<IServiceScopeFactory>();
            m_Scope = factory.CreateScope();
            this.Services = m_Scope.ServiceProvider;
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_Scope.Dispose();
        }

        /// <summary> The <see cref="IServiceScope"/> for <see cref="Services"/> </summary>
        private IServiceScope m_Scope;

        /// <summary> Gets the services available when executing a test method. </summary>
        protected IServiceProvider Services { get; private set; }

        /// <summary> Configures services. </summary>
        protected virtual void ConfigureServices(IServiceCollection serviceCollection)
        {
        }
    }
}
