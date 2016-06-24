using System.Transactions;
using AppBoot.Repos.Ambients;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> ���ڹ��� <see cref="SessionScopedAttribute"/> �뱻���ǵ� <see cref="Controller"/>. </summary>
    /// <remarks> �ýӿ��� controller ʵ��. </remarks>
    public interface IDataScopedCallback
    {
        /// <summary> 
        /// �� Controller ��<see cref="SessionScopedAttribute"/> �ṩ���ڴ��� 
        /// <see cref="SessionScope"/> �� <see cref="ISessionScopeFactory"/>.
        /// </summary>
        ISessionScopeFactory SessionScopeFactory { get; }

        /// <summary>
        /// �� <see cref="SessionScopedAttribute"/> �� action ����ִ��ǰ����.
        /// controller ͨ���˷������洴���� <see cref="SessionScope"/>.
        /// </summary>
        void AfterEnter(SessionScope sessionScope);

        /// <summary>
        /// �� <see cref="SessionScopedAttribute"/> �� action ����ִ�к����.
        /// controller ͨ���˷����������� <see cref="SessionScope"/>.
        /// </summary>
        void BeforeExit(SessionScope sessionScope);

        void AfterEnter(TransactionScope transactionScope);

        void BeforeExit(TransactionScope transactionScope);
    }
}