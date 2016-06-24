using System;
using System.Linq;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Repos.Ambients;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 忽略当前的 <see cref="SessionScopedAttribute"/>. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IgnoreSessionScopedAttribute : ActionFilterAttribute
    {
    }

    /// <summary> 对 MVC Action 采用自动化的 <see cref="SessionScope"/>. </summary>
    /// <remarks>
    /// 对于应用了本属性的方法，其执行过程如下：
    /// <list type="number">
    /// <item>创建 <see cref="SessionScope"/>.</item>
    /// <item>执行 Action 方法.</item>
    /// <item>若执行正常, 则调用 <see cref="SessionScope.SaveChanges"/> 方法, 若有异常则不调用.</item>
    /// <item>释放 <see cref="SessionScope"/>.</item>
    /// </list>
    /// <para>当本属性应用在 Controller 上时，相当于所有的方法均应用此属性.
    /// 可以通过 <see cref="IgnoreSessionScopedAttribute"/> 忽略部分方法. </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionScopedAttribute : ActionFilterAttribute
    {
        public SessionScopedAttribute()
        {
            this.Order = FwDefaultFilterOrders.SessionScoped;
        }

        /// <summary> 设置在 Action 完成时是否自动调用 <see cref="SessionScope.SaveChanges"/>. </summary>
        /// <remarks> 此方法的语义依赖于 <see cref="ISession"/> 的实现. 
        /// 在 <c>FineWork.Web.WebApi</c> 中, <see cref="FineWork.Data.Aef.FineWorkDbContext"/> 
        /// 在数据更改后总是立即提交到数据库中. </remarks>
        public bool SaveChanges { get; set; } = true;

        private SessionScope m_SessionScope;

        private IDataScopedCallback m_Callback;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (context.ActionDescriptor == null) return;
            if (context.ActionDescriptor.FilterDescriptors == null) return;
            if (context.IsActionDefined<IgnoreSessionScopedAttribute>()) return;

            m_Callback = context.Controller as IDataScopedCallback;
            if (m_Callback == null) return;
            if (m_Callback.SessionScopeFactory == null) return;

            m_SessionScope = m_Callback.SessionScopeFactory.CreateScope();
            m_Callback.AfterEnter(m_SessionScope);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (m_SessionScope != null)
            {
                try
                {
                    if (context.Exception == null)
                    {
                        if (this.SaveChanges)
                        {
                            m_SessionScope.SaveChanges();
                        }
                    }
                    this.m_Callback.BeforeExit(m_SessionScope);
                }
                finally
                {
                    m_SessionScope.Dispose();
                }
            }

            base.OnActionExecuted(context);
        }
    }
}