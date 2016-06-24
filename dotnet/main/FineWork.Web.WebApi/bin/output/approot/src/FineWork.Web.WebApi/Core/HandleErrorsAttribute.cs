using System;
using FineWork.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    /// <summary> 处理请求处理过程中的异常. </summary>
    /// <remarks> 对异常进行以下处理:
    /// <list type="bullet">
    /// <item> 对 <see cref="FineWorkException"/>, 返回 Http status code 5xx, 并提供详细的错误信息. </item>
    /// <item> 对其他类型的异常返回系统默认的结果. </item>
    /// </list>
    /// 本属性并不处理 ModelState 的错误, ModelState 错误由 <see cref="ValidateModelStateAttribute"/> 处理.
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