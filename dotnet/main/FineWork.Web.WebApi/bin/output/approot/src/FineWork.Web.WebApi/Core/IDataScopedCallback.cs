using System.Transactions;
using AppBoot.Repos.Ambients;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 用于关联 <see cref="SessionScopedAttribute"/> 与被其标记的 <see cref="Controller"/>. </summary>
    /// <remarks> 该接口由 controller 实现. </remarks>
    public interface IDataScopedCallback
    {
        /// <summary> 
        /// 由 Controller 向<see cref="SessionScopedAttribute"/> 提供用于创建 
        /// <see cref="SessionScope"/> 的 <see cref="ISessionScopeFactory"/>.
        /// </summary>
        ISessionScopeFactory SessionScopeFactory { get; }

        /// <summary>
        /// 由 <see cref="SessionScopedAttribute"/> 在 action 方法执行前调用.
        /// controller 通过此方法保存创建的 <see cref="SessionScope"/>.
        /// </summary>
        void AfterEnter(SessionScope sessionScope);

        /// <summary>
        /// 由 <see cref="SessionScopedAttribute"/> 在 action 方法执行后调用.
        /// controller 通过此方法清除保存的 <see cref="SessionScope"/>.
        /// </summary>
        void BeforeExit(SessionScope sessionScope);

        void AfterEnter(TransactionScope transactionScope);

        void BeforeExit(TransactionScope transactionScope);
    }
}