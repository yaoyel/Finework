using System;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;

namespace FineWork.Web.WebApi.Common
{
    public static class HttpUtil
    {

        private static IHttpContextAccessor m_HttpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor) 
        {
            m_HttpContextAccessor = httpContextAccessor; 
        }

        public static HttpContext HttpContext
        {
            get { return m_HttpContextAccessor.HttpContext; }
        }

        public static String GetUri(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return UriHelper.Encode(request.Scheme, request.Host, request.PathBase, request.Path,
                request.QueryString);
        }

        /// <summary> 检查 Action 方法上是否有类型为 <typeparamref name="T"/> 的属性. </summary>
        public static bool IsActionDefined<T>(this ActionExecutingContext context) where T : Attribute
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            bool markedWithNoSession = context.ActionDescriptor.FilterDescriptors.Any(descriptor =>
                descriptor.Filter is T);

            return markedWithNoSession;
        }
    }
}