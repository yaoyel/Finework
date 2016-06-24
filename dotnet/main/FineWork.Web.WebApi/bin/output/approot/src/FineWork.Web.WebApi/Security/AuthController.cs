using System; 
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims; 
using Microsoft.AspNet.Authentication.OAuthBearer;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc; 
using AppBoot.Repos.Ambients; 
using FineWork.Security;  
using FineWork.Web.WebApi.Core;
using Microsoft.Extensions.OptionsModel; 

namespace FineWork.Web.WebApi.Security
{  
    [Route("api/Auth")]
    [AllowAnonymous]
    public class AuthController : FwApiController
    {
        public AuthController(ISessionScopeFactory sessionScopeFactory, 
            IOptions<OAuthBearerAuthenticationOptions> bearerOptions, 
            SigningCredentials signingCredentials, 
            IAccountManager accountManager)
            :base(sessionScopeFactory)
        {
            //this.m_BearerOptions = bearerOptions.Options;
            this.m_SigningCredentials = signingCredentials;
            this.m_AccountManager = accountManager;
            this.m_SessionScopeFactory = sessionScopeFactory;
        }

        [HttpGet("Info")]
        public String Info()
        {
            return this.GetType().FullName;
        }

        private readonly OAuthBearerAuthenticationOptions m_BearerOptions;
        private readonly SigningCredentials m_SigningCredentials;
        private readonly IAccountManager m_AccountManager;
        private readonly ISessionScopeFactory m_SessionScopeFactory;

        [HttpGet("Token")] 
        public IActionResult Token(string phoneNumber, string pwd)
        {
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber));

            if (string.IsNullOrEmpty(pwd)) throw new ArgumentException(nameof(pwd));

            var account = m_AccountManager.FindAccountByPhoneNumber(phoneNumber);
            if (account == null || account.Password != pwd)
                return HttpUnauthorized();

            ClaimsIdentity identity = SecurityHelper.CreateIdentity(account, "Bearer");

            var handler = m_BearerOptions.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().First();
            var securityToken = handler.CreateToken(
                issuer: m_BearerOptions.TokenValidationParameters.ValidIssuer,
                audience: m_BearerOptions.TokenValidationParameters.ValidAudience,
                signingCredentials: m_SigningCredentials,
                subject: identity);
            var token = handler.WriteToken(securityToken);

            SecurityToken parsedToken;
            var parsedPrincipal = handler.ValidateToken(token, m_BearerOptions.TokenValidationParameters,
                out parsedToken);
            if (parsedPrincipal.Identity.Name != account.Name) throw new InvalidOperationException();
            return new ObjectResult(token);
        }

        [Authorize("Bearer")]
        [HttpGet("Parse")]
        public String Parse()
        {
            return this.User != null ? this.User.Identity.Name : "anonymous";
        }
    }
}
