using System;
using System.Security.Claims;
using FineWork.Common;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Core
{
    public class HttpErrorInfo
    {
        public String RequestUrl { get; set; }

        public ClaimsPrincipal User { get; set; }

        /// <summary> 用户友好的提示. </summary>
        /// <remarks> 对于 <see cref="FineWorkException"/>, 其值为 <see cref="FineWorkException.FriendlyMessage"/>. </remarks>
        public String Message { get; set; }

        public Exception Exception { get; set; }

        public static HttpErrorInfo Create(ExceptionContext context)
        {
            var info = new HttpErrorInfo();

            // HttpContext cannot be serialized to JSON because it has circular property references and IIS internal data
            info.RequestUrl = context.HttpContext?.Request?.GetUri();
            info.User = context.HttpContext?.User;
            info.Message = GetExceptionMessage(context.Exception);
            info.Exception = context.Exception;

            return info;
        }

        private static String GetExceptionMessage(Exception exception)
        {
            if (exception == null) return null;
            IFriendlyMessage friendly = exception as IFriendlyMessage;
            if (friendly != null)
            {
                return friendly.FriendlyMessage;
            }
            return exception.Message;
        }
    }
}