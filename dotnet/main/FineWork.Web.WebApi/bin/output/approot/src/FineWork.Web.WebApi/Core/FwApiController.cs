using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Transactions;
using AppBoot.Repos.Ambients;
using FineWork.Security;
using JetBrains.Annotations;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> The base class for all FineWork WebAPI controllers. </summary>
    [HandleErrors]
    [ValidateModelState]
    [DataScoped(false)]
    public class FwApiController : Controller, IDataScopedCallback
    {
        public FwApiController()
            : this(null)
        {
        }

        public FwApiController([CanBeNull] ISessionScopeFactory sessionScopeFactory)
        {
            this.SessionScopeFactory = sessionScopeFactory;
        }

        protected internal ISessionScopeFactory SessionScopeFactory { get; }

        /// <summary> Gets the <see cref="SessionScope"/> provided by <see cref="DataScopedAttribute"/>. </summary>
        protected internal SessionScope AutoSessionScope { get; private set; }

        ISessionScopeFactory IDataScopedCallback.SessionScopeFactory
        {
            get { return this.SessionScopeFactory; }
        }

        void IDataScopedCallback.AfterEnter(SessionScope sessionScope)
        {
            if (sessionScope == null) throw new ArgumentNullException(nameof(sessionScope));

            this.AutoSessionScope = sessionScope;
        }

        void IDataScopedCallback.BeforeExit(SessionScope sessionScope)
        {
            if (sessionScope == null) throw new ArgumentNullException(nameof(sessionScope));
            if (sessionScope == this.AutoSessionScope)
            {
                this.AutoSessionScope = null;
            }
        }

        /// <summary> Gets the <see cref="TransactionScope"/> provided by <see cref="DataScopedAttribute"/>. </summary>
        protected internal TransactionScope AutoTransactionScope { get; private set; }

        void IDataScopedCallback.AfterEnter(TransactionScope transactionScope)
        {
            if (transactionScope == null) throw new ArgumentNullException(nameof(transactionScope));

            this.AutoTransactionScope = transactionScope;
        }

        void IDataScopedCallback.BeforeExit(TransactionScope transactionScope)
        {
            if (transactionScope == null) throw new ArgumentNullException(nameof(transactionScope));
            if (transactionScope == this.AutoTransactionScope)
            {
                this.AutoTransactionScope = null;
            }
        }


        /// <summary> 获取相关联的 <see cref="IAccount.Id"/>. </summary>
        /// <exception cref="AuthenticationException"> 若不存在相应的 <see cref="IAccount.Id"/>. </exception>
        public Guid AccountId
        {
            get { return this.User.GetAccountId(); }
        }

        public String AccountName
        {
            get { return this.User.GetAccountName(); }
        }

        /// <summary> 用于在单元测试中设置当前用户. </summary>
        public void InternalSetAccount(IAccount account)
        {
            var identity = SecurityHelper.CreateIdentity(account, "Bearer");
            var principal = new ClaimsPrincipal(identity);
            InternalSetUser(principal);
        }

        /// <summary> 用于在单元测试中设置当前用户. </summary>
        public void InternalSetUser(ClaimsPrincipal principal)
        {
            //当用于 unit test 时, ActionContext.HttpContext 可能为 null.
            if (this.ActionContext.HttpContext == null)
            {
                this.ActionContext.HttpContext = new DefaultHttpContext();
            }
            this.ActionContext.HttpContext.User = principal;
        }
    }
}