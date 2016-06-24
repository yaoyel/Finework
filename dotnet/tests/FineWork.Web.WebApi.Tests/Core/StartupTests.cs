using System;
using System.IO;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace FineWork.Web.WebApi.Core
{
    public class StartupTests
    {
        [Fact]
        public void Services_can_be_configured()
        {
            //var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
            //Console.WriteLine($"AppDomain.BaseDirectory {appBaseDir}");
            var dir = Directory.GetCurrentDirectory();
            Console.WriteLine($"Directory.GetCurrentDirectory {dir}");

            IHostingEnvironment env = new HostingEnvironment();
         
            Startup startup = new Startup(env);
            
            var cfg = startup.Configuration;
            env.Initialize(dir, cfg);
            var cs = cfg.GetSection("ConnectionStrings:FineWork");
            Assert.NotNull(cs);

            ServiceCollection services = new ServiceCollection();
            startup.ConfigureServices(services);

            var sp = services.BuildServiceProvider();
            var sessionProvider = sp.GetRequiredService<ISessionProvider<AefSession>>();
            Assert.NotNull(sessionProvider);
        }

        /*
        [Fact]
        public async Task FoosController_can_be_tested_with_TestServer()
        {
            Action<IApplicationBuilder> _app;
            Action<IServiceCollection> _services;
            var environment = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();

            var host = new HostingEnvironment();
            var startup = new Startup(host);
            _app = ab => startup.Configure(ab, host);
            _services = startup.ConfigureServices;

            var server = TestServer.Create(_app, _services);
            var client = server.CreateClient();

            var response = await client.GetAsync("http://localhost/api/values");
            Assert.NotNull(response);
        }*/
    }
}
