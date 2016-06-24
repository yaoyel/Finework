using System;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading;
using System.Transactions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Security;
using JetBrains.Annotations;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> The base class for all FineWork WebAPI controllers. </summary>
    [ServiceFilter(typeof(HandleErrorsAttribute))]
    [ValidateModelState]
    [DataScoped(false)]
    public class FwApiController : Controller, IDataScopedCallback
    {
        public FwApiController(ISessionProvider<AefSession> sessionProvider)
        {
            m_SessionProvider = sessionProvider;
        }

        private readonly ISessionProvider<AefSession> m_SessionProvider;

        ISessionProvider<AefSession> IDataScopedCallback.SessionProvider
        {
            get { return m_SessionProvider; }
        }

        protected internal AefSession AutoSession { get; private set; }

        void IDataScopedCallback.AfterEnter(AefSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            this.AutoSession = session;
        }

        void IDataScopedCallback.BeforeExit(AefSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (Object.ReferenceEquals(this.AutoSession, session))
            {
                this.AutoSession = null;
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