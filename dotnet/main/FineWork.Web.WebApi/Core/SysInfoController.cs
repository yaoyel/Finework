using System;
using System.Dynamic;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Runtime;

namespace FineWork.Web.WebApi.Core
{
    [Route("api/SysInfo")]
    public class SysInfoController : FwApiController
    {
        public SysInfoController(IApplicationEnvironment environment)
            :base(null)
        {
            if (environment == null) throw new ArgumentNullException(nameof(environment));
            this.Enviornment = environment;
        }

        private IApplicationEnvironment Enviornment { get; }

        [HttpGet]
        public IActionResult Get()
        {
            dynamic info = new ExpandoObject();
            info.ApplicationDomainId = AppDomain.CurrentDomain.Id;
            info.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.ApplicationIdentity = AppDomain.CurrentDomain.ApplicationIdentity;
            info.ApplicationBasePath = this.Enviornment.ApplicationBasePath;
            info.ApplicationName = this.Enviornment.ApplicationName;
            info.RuntimeFramework = this.Enviornment.RuntimeFramework.FullName;
            return new ObjectResult(info);
        }
    }
}
