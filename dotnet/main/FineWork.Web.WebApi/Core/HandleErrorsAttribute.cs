using System;
using FineWork.Common;
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

        private readonly ILogger _logger;
        private readonly string _storageConnectionString;

        public HandleErrorsAttribute(ILoggerFactory loggerfactory,IConfiguration config)
        {
            _logger = loggerfactory.CreateLogger<HandleErrorsAttribute>();
            _storageConnectionString = config["AzureSettings:StorageConnectionString"];
        }


        public HandleErrorsAttribute()
        {
            this.Order = FwDefaultFilterOrders.HandleErrors;
        }

        public override void OnException(ExceptionContext context)
        {
            HandleException(context);

            _logger.LogInformation(context.HttpContext.Request.Path);
            _logger.LogError(3,"ServerError",context.Exception);
            LogExceptionByBlob(context.Exception);
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

        private void LogExceptionByBlob(Exception ex)
        { 
            var container = CloudStorageAccount.Parse(_storageConnectionString)
                  .CreateCloudBlobClient().GetContainerReference("errors");
            container.CreateIfNotExists();
            var blob = container.GetBlockBlobReference($"error-{$"chrdapi"}-{DateTime.Now.ToString("G")}");
            blob.UploadText(ex.ToString());
        }
    }
}