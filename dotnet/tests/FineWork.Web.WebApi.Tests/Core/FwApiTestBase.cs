using System;
using AppBoot.Common;
using FineWork.Core;
using FineWork.Web.WebApi.Tests.Colla;
using Microsoft.Extensions.DependencyInjection;

namespace FineWork.Web.WebApi.Tests.Core
{
    public abstract class FwApiTestBase : DisposableBase
    {
        /// <summary> 用于测试的假手机号. </summary>
        protected const String FakePhoneNumber = "11111111111";

        /// <summary>  Creates an instance. </summary>
        /// <param name="bed"> The <see cref="FwApiBed"/> injected by xUnit. </param>
        protected FwApiTestBase(FwApiBed bed)
        {
            if (bed == null) throw new ArgumentNullException(nameof(bed));

            this.Bed = bed;

            var services = this.Bed.ServiceCollection.BuildServiceProvider();
            this.Services = new FwFakerServices(services);
        }

        protected override void DoDispose(bool disposing)
        {
            if (disposing)
            {
                this.Services.Dispose();
            }
            base.DoDispose(disposing);
        }

        protected FwApiBed Bed { get; private set; }

        /// <summary> Gets the services. </summary>
        /// <remarks> 
        /// The <see cref="IServiceProvider"/> is created for each test method 
        /// for the maximum test isolation. 
        /// </remarks>
        protected FwFakerServices Services { get; private set; }
    }
}