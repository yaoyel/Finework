using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FineWork.Web.WebApi.Core;
using FineWork.Web.WebApi.Tests.Core;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace FineWork.Web.WebApi.Core
{
    /* Wait for the last ASP.net 5 beta */
    /*  
    [DataScoped]
    public class BaseController : Controller, IDataScopedCallback
    {
        public BaseController(ISessionScopeFactory factory)
        {
            this.SessionScopeFactory = factory;
        }

        public ISessionScopeFactory SessionScopeFactory { get; private set; }

        public bool InSessionScoped { get; set; }

        public void AfterEnter(SessionScope sessionScope)
        {
            InSessionScoped = true;
        }

        public void BeforeExit(SessionScope sessionScope)
        {
            InSessionScoped = false;
        }

        public bool InTransactionScope { get; private set; }

        public void AfterEnter(TransactionScope transactionScope)
        {
            InTransactionScope = true;
        }

        public void BeforeExit(TransactionScope transactionScope)
        {
            InTransactionScope = false;
        }
    }

    public class DerivedController : BaseController
    {
        public DerivedController(ISessionScopeFactory factory) 
            : base(factory)
        {
        }

        [DataScoped]
        public void DataMethod()
        {
        }
    }

    [Collection(FwApiBed.Name)]
    public class DataScopedAttributeTests : FwApiTestBase
    {
        public DataScopedAttributeTests(FwApiBed bed) 
            : base(bed)
        {
        }

        [Fact]
        public void DataMethod_should_in_scopes()
        {
            ActionContext actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary());

            var filterDescriptors = new List<FilterDescriptor>();
            filterDescriptors.Add(new FilterDescriptor(new DataScopedAttribute(), FilterScope.Action));

            actionContext.ActionDescriptor.FilterDescriptors = filterDescriptors;
            var invokerFactory = this.Services.GetRequiredService<IActionInvokerFactory>();
            var invoker = invokerFactory.CreateInvoker(actionContext);
            invoker.InvokeAsync().RunSynchronously();
        }
    }*/
}
