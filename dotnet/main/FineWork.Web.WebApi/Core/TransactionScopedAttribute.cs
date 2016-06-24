using System;
using System.Transactions;
using AppBoot.Transactions;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 忽略当前的 <see cref="TransactionScopedAttribute"/>. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreTransactionScopedAttribute : ActionFilterAttribute
    {
    }

    /// <summary> 对 MVC Action 采用自动化的 <see cref="TransactionScope"/>. </summary>
    /// <remarks>
    /// 对于应用了本属性的方法，其执行过程如下：
    /// <list type="number">
    /// <item>创建 <see cref="TransactionScope"/>.</item>
    /// <item>执行 Action 方法.</item>
    /// <item>若执行正常，则调用 <see cref="TransactionScope.Complete"/> 方法, 若有异常则不调用.</item>
    /// <item>释放 <see cref="TransactionScope"/>.</item>
    /// </list>
    /// <para>当本属性应用在 Controller 上时，相当于所有的方法均应用此属性.
    /// 可以通过 <see cref="IgnoreDataScopedAttribute"/> 忽略部分方法. </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TransactionScopedAttribute : ActionFilterAttribute
    {
        public TransactionScopedAttribute()
        {
            this.Order = FwDefaultFilterOrders.TransactionScoped;
        }

        private TransactionScope m_Scope;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.IsActionDefined<IgnoreTransactionScopedAttribute>()) return;

            this.m_Scope = TxManager.Acquire();
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (m_Scope != null)
            {
                try
                {
                    if (context.Exception != null)
                    {
                        m_Scope.Complete();
                    }
                }
                finally
                {
                    m_Scope.Dispose();
                }
            }

            base.OnActionExecuted(context);
        }
    }
}