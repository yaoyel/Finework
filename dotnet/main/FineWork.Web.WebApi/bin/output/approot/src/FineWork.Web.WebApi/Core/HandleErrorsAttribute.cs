using System;
using FineWork.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> ��������������е��쳣. </summary>
    /// <remarks> ���쳣�������´���:
    /// <list type="bullet">
    /// <item> �� <see cref="FineWorkException"/>, ���� Http status code 5xx, ���ṩ��ϸ�Ĵ�����Ϣ. </item>
    /// <item> ���������͵��쳣����ϵͳĬ�ϵĽ��. </item>
    /// </list>
    /// �����Բ������� ModelState �Ĵ���, ModelState ������ <see cref="ValidateModelStateAttribute"/> ����.
    /// </remarks>
    public class HandleErrorsAttribute : ExceptionFilterAttribute
    {
        public HandleErrorsAttribute()
        {
            this.Order = FwDefaultFilterOrders.HandleErrors;
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
        }

        private static void HandleException( ExceptionContext context, int httpStatusCode = 500)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var info = HttpErrorInfo.Create(context);
            context.Result = new ObjectResult(info)
            {
                StatusCode = httpStatusCode
            }; 
        }
    }
}