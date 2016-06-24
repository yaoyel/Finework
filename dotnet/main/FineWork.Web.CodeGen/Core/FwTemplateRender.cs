using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace FineWork.Web.CodeGen.Core
{
    public class FwTemplateRender<TModel> : IReferenceResolver
    {
        private const String m_PrimaryTemplateKey = "__PrimaryTemplateKey";

        public String Template { get; set; }

        public ICollection<CompilerReference> References { get; set; } = new List<CompilerReference>();

        public virtual String Render(TModel model)
        {
            var service = CreateService();
            var result = service.Run(m_PrimaryTemplateKey, typeof (TModel), model, null);
            return result;
        }

        protected virtual IRazorEngineService CreateService()
        {
            var config = CreateConfiguration();
            var service = RazorEngineService.Create(config);
            service.AddTemplate(m_PrimaryTemplateKey, new LoadedTemplateSource(Template));
            service.Compile(m_PrimaryTemplateKey, typeof(TModel));
            return service;
        }

        protected virtual ITemplateServiceConfiguration CreateConfiguration()
        {
            TemplateServiceConfiguration config = new TemplateServiceConfiguration();
            config.Language = Language.CSharp;
            config.EncodedStringFactory = new RawStringFactory();
            config.Debug = true;
            config.ReferenceResolver = this;
            return config;
        }

        public static FwTemplateRender<TModel> Create(String templatePath)
        {
            FwTemplateRender<TModel> result = new FwTemplateRender<TModel>();
            result.Template = File.ReadAllText(templatePath);
            return result;
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly IReferenceResolver m_DefaultResolver = new UseCurrentAssembliesReferenceResolver();

        public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies = null)
        {
            var list = m_DefaultResolver.GetReferences(context, includeAssemblies);
            list = References != null ? list.Union(this.References) : list;
            return list;
        }
    }
}
