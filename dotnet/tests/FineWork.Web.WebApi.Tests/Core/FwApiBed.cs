using System;
using System.IO;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace FineWork.Web.WebApi.Tests.Core
{
    /// <summary> The enviornment setup/cleanup for web api unit tests. </summary>
    /// <remarks> This class is created only once and is shared among all test classes.
    /// <see cref="http://xunit.github.io/docs/shared-context.html#collection-fixture"/> </remarks>
    [CollectionDefinition(Name)]
    public class FwApiBed : ICollectionFixture<FwApiBed>
    {
        public FwApiBed()
        {
            var dir = Directory.GetCurrentDirectory();
            HostingEnvironment env = new HostingEnvironment();
     

            Startup startup = new Startup(env); 
       
            env.Initialize(dir, startup.Configuration);
            startup.ConfigureServices(this.ServiceCollection);
        }

        public ServiceCollection ServiceCollection { get; } = new ServiceCollection();

        public const string Name = "FwApiBed";
    }
}