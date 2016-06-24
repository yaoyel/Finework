using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Linq;
namespace FineWork.Web.WebApi.Core
{
    /// <summary> 
    /// 检查 <see cref="ModelStateDictionary"/>, 
    /// 若 <see cref="ModelStateDictionary.IsValid"/> 为 <c>false</c>
    /// 则向调用方返回 <see cref="BadRequestObjectResult"/>.
    /// </summary>
    /// <remarks> <see cref="FwApiController"/> 已经应用了本属性，它的派生类不需要再加上这个属性了. </remarks>
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                context.Result = new BadRequestObjectResult(context.ModelState.Values
                    .SelectMany(p=>p.Errors).First(p=>p.ErrorMessage!="" || p.Exception!=null).ErrorMessage);
            }
        }
    }
}