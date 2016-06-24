using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using AppBoot.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Common
{
    [Route("api/v2/Foos")]
    public class RestfulFoosController : FwApiController
    {
        public RestfulFoosController()
            : base(null)
        {
        }

        #region Create

        //POST http://localhost:52851/api/v2/Foos
        [HttpPost()]
        public Object Create()
        {
            return new
            {
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        //POST http://localhost:52851/api/v2/Foos/Name
        [HttpPost("Name")]
        public IActionResult Create(String name)
        {
            Args.NotEmpty(name, nameof(name));
            return new ObjectResult(new FooModel
            {
                Id = 1,
                Name = $"Create(name) - {name}",
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            });
        }

        #endregion

        #region Update

        //PUT http://localhost:52851/api/v2/Foos/
        [HttpPut]
        public Object Update(int id, String name, String note)
        {
            return new
            {
                Id = id,
                Name = name,
                Note = note,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        //PUT http://localhost:52851/api/v2/Foos/name
        [HttpPut("name")]
        public Object UpdateName(int id, String name)
        {
            return new
            {
                Id = id,
                Name = name,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        //PUT http://localhost:52851/api/v2/Foos/note
        [HttpPut("note")]
        public Object UpdateNote(int id, String note)
        {
            return new
            {
                Id = id,
                Note = note,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        #endregion

        #region Delete

        //DELETE http://localhost:52851/api/v2/Foos/1
        [HttpDelete("{id:int}")]
        public Object Delete(int id)
        {
            return new
            {
                Id = id,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        //DELETE http://localhost:52851/api/v2/Foos/Tom
        //DELETE http://localhost:52851/api/v2/Foos/%E4%B8%AD%E4%BA%BA%E7%BD%91
        [HttpDelete("{name}")]
        public Object DeleteByName(String name)
        {
            return new
            {
                Name = name,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        #endregion

        #region Find Single

        //GET http://localhost:52851/api/v2/Foos/1
        [HttpGet("{id:int}")]
        public Object FindById(int id)
        {
            return new
            {
                Id = id,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        //GET http://localhost:52851/api/v2/Foos/Tom
        //GET http://localhost:52851/api/v2/Foos/%E4%B8%AD%E4%BA%BA%E7%BD%91
        [HttpGet("{name}")]
        public Object FindByName(String name)
        {
            return new 
            {
                Name = name,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        #endregion

        #region Find Single Releated

        [HttpGet("{id:int}/bars")]
        public Object FooBars(int id)
        {
            return new
            {
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        [HttpGet("{name}/bars")]
        public Object FooBars(String name)
        {
            return new
            {
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        #endregion

        [HttpGet("*")]                   
        public Object FetchAll()
        {
            return new
            {
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        [HttpGet("*/&Bar")]            
        public Object FetchFoosWithBar()
        {
            return new
            {
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        [HttpGet("*/{name}")]             
        public Object FetchFoosByName(String name)
        {
            return new
            {
                Name = name,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        [HttpGet("*&Bar/{name}")]
        public Object FetchFoosWithBarByName(String name)
        {
            return new
            {
                Name = name,
                Method = PrintMethod(MethodBase.GetCurrentMethod())
            };
        }

        public static string PrintMethod(MethodBase mi)
        {
            String[] paramDecls = mi.GetParameters()
                          .Select(p => $"{p.ParameterType.Name} {p.Name}")
                          .ToArray();
            String paramsDecl = String.Join(",", paramDecls);
            string signature = $"{mi.Name}({paramsDecl})";
            return signature;
        }
    }
}