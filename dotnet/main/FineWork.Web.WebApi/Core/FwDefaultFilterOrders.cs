using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> Defines the execution order for <see cref="IActionFilter"/>s centricly. </summary>
    /// <remarks> The action filter with the lowest value will be executed firstly. 
    /// <para> MSDN: https://msdn.microsoft.com/en-us/library/system.web.mvc.filterattribute.order(v=vs.118).aspx </para>
    /// <para>
    /// The Order property takes an integer value that must be 0 (the default) or greater, with one exception. 
    /// Omitting the Order property gives the filter an order value of -1, which indicates an unspecified order. 
    /// Any action filter in a scope whose Order property is set to -1 will be executed in an undetermined order, 
    /// but before the filters that have a specified order.
    /// </para>
    /// </remarks>
    public class FwDefaultFilterOrders
    {
        public const int HandleErrors = 10;

        public const int TransactionScoped = 20;

        public const int SessionScoped = 30;

        public const int DataScoped = 40;
    }
}
