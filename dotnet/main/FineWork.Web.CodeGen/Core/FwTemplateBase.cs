using RazorEngine.Templating;

namespace FineWork.Web.CodeGen.Core
{
    /// <summary>
    /// The base class for the generated template class.
    /// </summary>
    /// <remarks>
    /// See https://antaris.github.io/RazorEngine/IntellisenseAndResharper.html
    /// </remarks>
    public class FwTemplateBase<TModel> : TemplateBase<TModel>
    {
    }
}