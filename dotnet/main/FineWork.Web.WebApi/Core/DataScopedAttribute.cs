using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 忽略当前的 <see cref="DataScopedAttribute"/>. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class IgnoreDataScopedAttribute : ActionFilterAttribute
    {
    }

    /// <summary> 对 MVC Action 采用自动化的 数据访问. </summary>
    /// <remarks>
    /// <para>
    /// 当本属性应用在 Controller 上时，相当于所有的方法均应用此属性.
    /// 可以通过 <see cref="IgnoreDataScopedAttribute"/> 忽略部分方法. 
    /// </para>
    /// <para>
    /// 当 Controller 与 Action 均有 <see cref="DataScopedAttribute"/> 标记时,
    /// 则只有与 Action 上的属性被启用.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DataScopedAttribute : ActionFilterAttribute
    {
        public DataScopedAttribute()
            : this(true, true)
        {
        }

        public DataScopedAttribute(bool hasChanges)
            :this(hasChanges, hasChanges)
        {
        }

        public DataScopedAttribute(bool hasChanges, bool useTransaction)
        {
            this.HasChanges = hasChanges;
            this.UseTransaction = useTransaction;
            //this.Order = FwDefaultFilterOrders.SessionScoped;
        }

        /// <summary> 设置是否涉及数据更改. 默认为<c>true</c>.</summary>
        public bool HasChanges { get; set; }

        /// <summary> 设置是否使用事务. 默认为 <c>true</c> </summary>
        public bool UseTransaction { get; set; }

        /// <summary> 标记本实例是否启用. </summary>
        private bool IsEnabled { get; set; }

        private IDataScopedCallback Callback { get; set; }

        /// <summary>
        /// The session retrieved from the ISessionProvider{T} which is injected in controller
        /// </summary>
        /// <remarks>In finework, the session is disposed by the IoC container.</remarks>
        private AefSession Session { get; set; }

        private TransactionScope TransactionScope { get; set; } 
        

        public override void OnActionExecuting(ActionExecutingContext context)
        { 
            base.OnActionExecuting(context);

            if (context.ActionDescriptor == null) return;
            if (context.ActionDescriptor.FilterDescriptors == null) return;
            if (context.IsActionDefined<IgnoreDataScopedAttribute>()) return;

            var first = GetFirst(context.ActionDescriptor.FilterDescriptors);
            if (first != this) return;

            this.Callback = GetCallback(context);

            this.IsEnabled = true;

            if (this.UseTransaction)
            {
                this.TransactionScope = TxManager.Acquire();
                this.Callback.AfterEnter(TransactionScope);
            }

            this.Session = this.Callback.SessionProvider.GetSession();
            this.Callback.AfterEnter(this.Session);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        { 

            if (IsEnabled)
            {
                //当 Action 有修改且正常执行完成后，保存事务；当 ReadOnly = true 时 或 有异常发生时，回滚事务
                bool writeChanges = HasChanges && context.Exception == null;

                try
                {
                    if (this.Session != null)
                    {
                        if (writeChanges)
                        {
                            this.Session.SaveChanges();
                        }
                        this.Callback.BeforeExit(this.Session);
                    }
                    if (this.TransactionScope != null)
                    {
                        if (writeChanges)
                        {
                            this.TransactionScope.Complete();
                        }

                        this.Callback.BeforeExit(this.TransactionScope);
                    }
                }
                finally
                {
                    if (this.TransactionScope != null)
                    {
                        this.TransactionScope.Dispose();
                    }
                }
            }

            base.OnActionExecuted(context);
        }


        private static DataScopedAttribute GetFirst(IList<FilterDescriptor> filterDescriptors)
        {
            if (filterDescriptors == null) throw new ArgumentNullException(nameof(filterDescriptors));

            var dataScopeDescriptors = filterDescriptors.Where(x => x.Filter is DataScopedAttribute);
            //FirstDescriptor.Scope on Method is Less than on class
            var firstDescriptor = dataScopeDescriptors.OrderByDescending(x => x.Scope).FirstOrDefault();
            if (firstDescriptor != null)
            {
                return (DataScopedAttribute)firstDescriptor.Filter;
            }
            return null;
        }

        private static readonly String m_CallbackName = typeof(IDataScopedCallback).FullName;
        private static readonly String m_FactoryName = typeof(ISessionProvider<AefSession>).FullName;

        private static IDataScopedCallback GetCallback(ActionExecutingContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Controller == null)
            {
                throw new InvalidOperationException("Unexpected context, the context.Controller is null.");
            }

            var controllerName = context.Controller.GetType().FullName;

            var result = context.Controller as IDataScopedCallback;
            if (result == null)
            {
                throw new InvalidOperationException($"The {controllerName} should implement {m_CallbackName}.");
            }
            if (result.SessionProvider == null)
            {
                throw new InvalidOperationException($"The {controllerName} should provide an {m_FactoryName}.");
            }
            return result;
        }
    }
}