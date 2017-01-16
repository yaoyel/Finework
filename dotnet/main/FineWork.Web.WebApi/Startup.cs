using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Server; 
using FineWork.Core;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
namespace FineWork.Web.WebApi
{
    public class Startup
    {
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);

        public Startup(IHostingEnvironment hostEnv)
        {
            // Setup configuration sources.
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(hostEnv.WebRootPath);
            configBuilder
                .AddJsonFile(@"Configs\connectionStrings.json", false)
                .AddJsonFile(@"Configs\azureSettings.json", false)
                .AddJsonFile(@"Configs\pushSettings.json", false)
                .AddJsonFile(@"Configs\leancloudSettings.json", false);

            configBuilder.AddEnvironmentVariables(); 
            Configuration = configBuilder.Build();
            RSAParameters keyParams = RSAKeyUtil.GetKeyParameters(Path.Combine(hostEnv.WebRootPath,"Configs","RSA.Key")); 
            
            m_Key = new RsaSecurityKey(keyParams);
        }

        public IConfiguration Configuration { get; set; }
         
        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInstance<IConfiguration>(Configuration);

            services.AddMvc().AddMvcOptions(options =>
            {
                var jsonOutput = options
                    .OutputFormatters
                    .OfType<JsonOutputFormatter>()
                    .First();

                jsonOutput.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; 
                jsonOutput.SerializerSettings.ContractResolver = new SkipDefaultPropertyValuesContractResolver();
            });

            var cs = Configuration["ConnectionStrings:FineWork"];
            var storagecs = Configuration["AzureSettings:StorageConnectionString"];
            var jPushKey = Configuration["JPush:AppKey"];
            var jPushSecret = Configuration["JPush:MasterSecret"];
            var lcAppId = Configuration["LeanCloud:AppId"];
            var lcAppKey = Configuration["LeanCloud:AppKey"];

            services.AddFileManager(storagecs)
                .AddDbSession(cs)
                .AddSessionProvider()
                .AddFineWorkCoreServices()
                .AddJPushClient(jPushKey, jPushSecret)
                .AddImClient(Configuration)
                .AddSmsService(lcAppId, lcAppKey);  

            SigningCredentials sc = new SigningCredentials(m_Key, SecurityAlgorithms.RsaSha256Signature);
            services.AddInstance(sc);

            services.Configure<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters.IssuerSigningKey = m_Key;
                options.TokenValidationParameters.ValidAudience = m_TokenAudience;
                options.TokenValidationParameters.ValidIssuer = m_TokenIssuer;
                options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());
            }); 
            

            services.AddInstance(new JwtBearerOptions());

            services.AddAuthorization(auth =>
            { 
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build()); 
            });

            services.AddTransient<HandleErrorsAttribute>(); 
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Error; 
            loggerFactory.AddConsole(); 
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".apk", "application/x-msdownload"); 
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

            app.UseIISPlatformHandler();
      
            app.Use(next => async context =>
            {
                try
                {
                    await next(context);
                }

                catch
                {
                 
                    if (context.Response.HasStarted)
                    {
                        throw;
                    }

                    context.Response.StatusCode = 401;
                }
            });

            app.UseJwtBearerAuthentication(options =>
            {
                options.TokenValidationParameters.IssuerSigningKey = m_Key;
                options.TokenValidationParameters.ValidAudience = m_TokenAudience;
                options.TokenValidationParameters.ValidIssuer = m_TokenIssuer;
                options.TokenValidationParameters.ValidateSignature = true;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.RefreshOnIssuerKeyNotFound = true;
            });

         

            app.UseCors("Bearer");
            app.UseMvc(); 
            HttpUtil.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());

        }

        private const string m_TokenAudience = "Myself";
        private const string m_TokenIssuer = "MyProject";
        private static RsaSecurityKey m_Key; 
    }
}
