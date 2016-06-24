using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection; 
namespace FineWork.Web.WebApi.Security
{
    public static class OAuthBearerAuthenticationExtensions
    {
        public static String GenerateToken(this JwtSecurityTokenHandler tokenHandler,
            JwtBearerOptions options, SigningCredentials signingCredentials, ClaimsIdentity identity)
        {
            if (tokenHandler == null) throw new ArgumentNullException(nameof(tokenHandler));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (signingCredentials == null) throw new ArgumentNullException(nameof(signingCredentials));
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            var token = tokenHandler.CreateToken(
                issuer: options.TokenValidationParameters.ValidIssuer,
                audience: options.TokenValidationParameters.ValidAudience,
                signingCredentials: signingCredentials,
                subject: identity);
            return tokenHandler.WriteToken(token);
        }

        private static readonly string TokenIssuer = "FineWork.Issuer";
        private static readonly string TokenAudience = "FineWork.Audience";

        public static IServiceCollection ConfigureOAuthBearerAuthenticationOptions(this IServiceCollection services,
            String rsaKeyPath)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            //Configure OAuthBearerAuthenticationOptions
            RsaSecurityKey key = LoadRsaSecurityKey(rsaKeyPath);
            services.Configure<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters.ValidIssuer = TokenIssuer;
                options.TokenValidationParameters.ValidAudience = TokenAudience;
                options.TokenValidationParameters.IssuerSigningKey = key; 
                options.AutomaticAuthenticate = true;
                
            });

            //Register SigningCredentials used by AuthController
            var signingCredentials = new SigningCredentials(key,SecurityAlgorithms.RsaSha256Signature);
            services.AddInstance(signingCredentials);

            return services;
        }

        /// <summary> Loads the RsaSecurityKey from <c>Configs\RSA.Key</c>. </summary>
        public static RsaSecurityKey LoadRsaSecurityKey(String path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentException("path is null or empty.", nameof(path));

            using (var textReader = new System.IO.StreamReader(path))
            {
                RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
                cryptoServiceProvider.FromXmlString(textReader.ReadToEnd());

                var key = new RsaSecurityKey(cryptoServiceProvider.ExportParameters(true));
                return key;
            }
        }
    }
}