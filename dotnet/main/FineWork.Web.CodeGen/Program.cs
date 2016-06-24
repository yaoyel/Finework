using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Web.CodeGen.Core;
using Microsoft.Framework.Runtime;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace FineWork.Web.CodeGen
{
    public class Program
    {
        public Program(IApplicationEnvironment environment)
        {
            this.Enviornment = environment;
        }

        private IApplicationEnvironment Enviornment { get; }

        public static readonly CompilerReference FineWork_Web_CodeGen_dll = CompilerReference.From(
            @"C:\Yaojian\Projects\FineWork\devel\sources\dotnet\main\artifacts\bin\FineWork.Web.CodeGen\Debug\dnx451\FineWork.Web.CodeGen.dll");

        public void Main(string[] args)
        {
            var path = this.Enviornment.ApplicationBasePath + @"\Templates\HelloWorld.cshtml";

            FwTemplateRender<CustomInfo> t = new FwTemplateRender<CustomInfo>();
            t.Template = File.ReadAllText(path);
            t.References.Add(FineWork_Web_CodeGen_dll);
            var result = t.Render(new CustomInfo {Name = "Yaojian"});

            Console.Out.WriteLine(result);
            Console.ReadKey();
        }
    }
}
