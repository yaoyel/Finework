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
        public static String GetUri(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return UriHelper.Encode(request.Scheme, request.Host, request.PathBase, request.Path,
                request.QueryString);
        }

        /// <summary> ��� Action �������Ƿ�������Ϊ <typeparamref name="T"/> ������. </summary>
        public static bool IsActionDefined<T>(this ActionExecutingContext context) where T : Attribute
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            bool markedWithNoSession = context.ActionDescriptor.FilterDescriptors.Any(descriptor =>
                descriptor.Filter is T);

            return markedWithNoSession;
        }
    }
}