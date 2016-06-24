using System.Transactions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> ���ڹ��� <see cref="DataScopedAttribute"/> �뱻���ǵ� <see cref="Controller"/>. </summary>
    /// <remarks> �ýӿ��� controller ʵ��. </remarks>
    public interface IDataScopedCallback
    {
        /// <summary> 
        /// �� Controller ��<see cref="DataScopedAttribute"/> �ṩ���ڴ��� 
        /// <see cref="AefSession"/> �� <see cref="ISessionProvider"/>.
        /// </summary>
        ISessionProvider<AefSession> SessionProvider { get; }

        /// <summary>
        /// �� <see cref="DataScopedAttribute"/> �� action ����ִ��ǰ����.
        /// controller ͨ���˷������洴���� <see cref="AefSession"/>.
        /// </summary>
        void AfterEnter(AefSession session);

        /// <summary>
        /// �� <see cref="DataScopedAttribute"/> �� action ����ִ�к����.
        /// controller ͨ���˷����������� <see cref="AefSession"/>.
        /// </summary>
        void BeforeExit(AefSession session);

        void AfterEnter(TransactionScope transactionScope);

        void BeforeExit(TransactionScope transactionScope);
    }
}