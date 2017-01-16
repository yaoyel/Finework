using System;
using FineWork.Common;
using FineWork.Logging;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

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

        private readonly ILogger m_logger;  

        public HandleErrorsAttribute()
        {
            m_logger = LogManager.GetLogger(typeof(HandleErrorsAttribute));
            this.Order = FwDefaultFilterOrders.HandleErrors;
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);

            m_logger.LogInformation(context.HttpContext.Request.Path);
            m_logger.LogError(3,"ServerError",context.Exception); 
        }

        private static void HandleException( ExceptionContext context, int httpStatusCode = 500)
        {
            if (context == null) throw new ArgumentNullException(nameof(context)); 
            
            var info = HttpErrorInfo.Create(context);
            int statusCode = 500; 

            context.Result = new ObjectResult(info)
            {
              StatusCode=statusCode
            }; 
        } 
    }
}