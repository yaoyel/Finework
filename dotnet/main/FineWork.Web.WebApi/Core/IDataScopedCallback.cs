using System.Transactions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 用于关联 <see cref="DataScopedAttribute"/> 与被其标记的 <see cref="Controller"/>. </summary>
    /// <remarks> 该接口由 controller 实现. </remarks>
    public interface IDataScopedCallback
    {
        /// <summary> 
        /// 由 Controller 向<see cref="DataScopedAttribute"/> 提供用于创建 
        /// <see cref="AefSession"/> 的 <see cref="ISessionProvider"/>.
        /// </summary>
        ISessionProvider<AefSession> SessionProvider { get; }

        /// <summary>
        /// 由 <see cref="DataScopedAttribute"/> 在 action 方法执行前调用.
        /// controller 通过此方法保存创建的 <see cref="AefSession"/>.
        /// </summary>
        void AfterEnter(AefSession session);

        /// <summary>
        /// 由 <see cref="DataScopedAttribute"/> 在 action 方法执行后调用.
        /// controller 通过此方法清除保存的 <see cref="AefSession"/>.
        /// </summary>
        void BeforeExit(AefSession session);

        void AfterEnter(TransactionScope transactionScope);

        void BeforeExit(TransactionScope transactionScope);
    }
}