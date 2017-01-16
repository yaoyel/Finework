using System; 
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using FineWork.Security;
using FineWork.Security.Passwords;
using FineWork.Web.WebApi.Core;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Authentication.JwtBearer;
namespace FineWork.Web.WebApi.Security
{  
    [Route("api/Auth")]
    [AllowAnonymous]
    public class AuthController : FwApiController
    {
        public AuthController(ISessionProvider<AefSession> sessionProvider,
            IOptions<JwtBearerOptions> bearerOptions, 
            SigningCredentials signingCredentials, 
            IAccountManager accountManager,
            IPasswordService passwordService)
            :base(sessionProvider)
        {
            this.m_BearerOptions = bearerOptions.Value;
            this.m_SigningCredentials = signingCredentials;
            this.m_AccountManager = accountManager;
            this.m_PasswordService = passwordService;
        }

        [HttpGet("Info")]
        public String Info()
        {
            return this.GetType().FullName;
        }

        private readonly JwtBearerOptions m_BearerOptions;
        private readonly SigningCredentials m_SigningCredentials;
        private readonly IAccountManager m_AccountManager;
        private readonly IPasswordService m_PasswordService;

        [HttpGet("Token")]
        [AllowAnonymous]
        public IActionResult Token(string phoneNumber, string pwd)
        { 
            if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber));

            if (string.IsNullOrEmpty(pwd)) throw new ArgumentException(nameof(pwd));

            var account = m_AccountManager.FindAccountByPhoneNumber(phoneNumber);
            if (account == null ||
                !m_PasswordService.Verify(account.PasswordFormat, pwd, account.Password, account.PasswordSalt))
                return HttpUnauthorized();
 

            ClaimsIdentity identity = SecurityHelper.CreateIdentity(account, "Bearer");

            var handler = m_BearerOptions.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().First();
            var securityToken = handler.CreateToken(
                issuer: m_BearerOptions.TokenValidationParameters.ValidIssuer,
                audience: m_BearerOptions.TokenValidationParameters.ValidAudience,
                signingCredentials: m_SigningCredentials,
                subject: identity,
                expires:DateTime.Now.AddMonths(1));
            var token = handler.WriteToken(securityToken);

            SecurityToken parsedToken;
            var parsedPrincipal = handler.ValidateToken(token, m_BearerOptions.TokenValidationParameters,
                out parsedToken);
            if (parsedPrincipal.Identity.Name != (account.Name??"")) throw new InvalidOperationException();
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
