using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using FineWork.Core;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Authentication.OAuthBearer;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json; 
namespace FineWork.Web.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment hostEnv)
        {
            // Setup configuration sources.
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(hostEnv.WebRootPath);
            configBuilder
                .AddJsonFile(@"Configs\connectionStrings.json", false)
                .AddJsonFile(@"Configs\azureSettings.json", false)
                .AddJsonFile(@"Configs\pushSettings.json", false)
                .AddJsonFile(@"Configs\imSettings.json", false);

            configBuilder.AddEnvironmentVariables();

            Configuration = configBuilder.Build();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMvcOptions(options =>
            {
                var jsonOutput = options
                    .OutputFormatters
                    .OfType<JsonOutputFormatter>()
                    .First();

                jsonOutput.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            var cs = Configuration.Get("ConnectionStrings:FineWork");
            var storagecs = Configuration.Get("AzureSettings:StorageConnectionString");
            var jPushKey = Configuration.Get("JPush:AppKey");
            var jPushSecret = Configuration.Get("JPush:MasterSecret");
            var lcAppId = Configuration.Get("LeanCloud:AppId");
            var lcAppKey = Configuration.Get("LeanCloud:AppKey");
            services.AddFileManager(storagecs)
                .AddDbSession(cs)
                .AddSessionScopeFactory()
                .AddAmbientSessionProvider()
                .AddFineWorkCoreServices()
                .AddJPushClient(jPushKey, jPushSecret)
                .AddLCClient(lcAppId, lcAppKey);
            

            RsaSecurityKey key = GetRsaSecurityKey();
            SigningCredentials sc = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature,
                SecurityAlgorithms.Sha256Digest);
            services.AddInstance(sc);

            services.Configure<OAuthBearerAuthenticationOptions>(options =>
            {
                options.TokenValidationParameters.IssuerSigningKey = key;
                options.TokenValidationParameters.ValidAudience = m_TokenAudience;
                options.TokenValidationParameters.ValidIssuer = m_TokenIssuer;
                options.SecurityTokenValidators = new List<ISecurityTokenValidator>();
                options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());
            });
             
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(OAuthBearerAuthenticationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            app.UseStaticFiles();
            //app.UseOAuthBearerAuthentication();
            app.UseOAuthAuthentication(option => { });
            app.UseCors("Bearer");
            //app.UseFwToken();
            app.UseMvc();
        }

        private const string m_TokenAudience = "Myself";
        private const string m_TokenIssuer = "MyProject";
        private static RsaSecurityKey m_Key;

        public static RsaSecurityKey GetRsaSecurityKey()
        {
            if (m_Key == null)
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider(2048);
                RSAParameters publicKey = provider.ExportParameters(true);
                m_Key = new RsaSecurityKey(publicKey);
            }
            return m_Key;
        }
    }
}
