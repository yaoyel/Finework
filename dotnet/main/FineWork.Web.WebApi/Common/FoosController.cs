using System;
using System.Collections.Generic;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Common
{
    public class FooModel
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public BarModel Bar { get; set; }

        public String Method { get; set; }

        public String TimeStamp { get; } = DateTime.Now.ToString("HH:mm:ss:ms");
    }

    public class BarModel
    {
        public String Info { get; set; }
    }

    [Route("api/Foos")]
    public class FoosController : FwApiController
    {
        public FoosController(ISessionProvider<AefSession> sessionProvider)
            :base(sessionProvider)
        {
        }

        [HttpPost("CreateFoo")] //Matches:  http://localhost:41969/api/Foo/CreateAccount
        public IActionResult CreateFoo(String name)
        {
            Args.NotEmpty(name, nameof(name));
            return new ObjectResult(new FooModel
            {
                Id = 1,
                Name = $"CreateFoo - {name}"
            });
        }

        [HttpPost("UpdateFoo")] //Matches:  http://localhost:41969/api/Foo/UpdateAccount
        public IActionResult UpdateFoo(String name)
        {
            Args.NotEmpty(name, nameof(name));
            return new ObjectResult(new FooModel
            {
                Id = 1,
                Name = $"UpdateFoo - {name}"
            });
        }

        [HttpPost("DeleteFoo")]     
        [HttpDelete]
        public IActionResult DeleteFoo(int id)
        {
            return new ObjectResult(new FooModel
            {
                Id = id,
                Name = $"DeleteFoo - {id}"
            });
        }

        [HttpGet("FindFoo")]
        public IActionResult FindFoo(int id)
        {
            return new ObjectResult(new FooModel
            {
                Id = 1,
                Name = $"FindFoo - {id}"
            });
        }

        [HttpGet("FindFooByName")]
        public IActionResult FindFooByName(String name)
        {
            return new ObjectResult(new FooModel
            {
                Id = 1,
                Name = $"FindFooByName - {name}"
            });
        }

        [HttpGet("FetchFoos")]                   
        public IEnumerable<FooModel> FetchFoos()
        {
            return CreateList("FetchFoos", 2, false);
        }

        [HttpGet("FetchFoosWithBar")]            
        public IEnumerable<FooModel> FetchFoosWithBar()
        {
            return CreateList("FetchFoosWithBarAll", 2, true);
        }

        [HttpGet("FetchFoosByName")]             
        public IEnumerable<FooModel> FetchFoosByName(String name)
        {
            Args.NotEmpty(name, nameof(name));
            return CreateList($"FetchFoosByName {name}", 2, false);
        }

        [HttpGet("FetchFoosWithBarByName")]
        public IEnumerable<FooModel> FetchFoosWithBarByName(String name)
        {
            Args.NotEmpty(name, nameof(name));
            return CreateList($"FetchFoosWithBarByName {name}", 2, true);
        }

        [HandleErrors]
        [HttpGet("ThrowFineWorkException")]
        public Object ThrowFineWorkException()
        {
            throw new FineWorkException("Long message").FriendlyMessage("ShortMessage");
        }

        private string GetDataScopedStatus()
        {
            var s = $"Has AutoTransactionScope : {this.AutoTransactionScope != null} , Has AutoSession : {this.AutoSession != null}";
            return s;
        }

        [HttpGet("DataScopedWithChangesAndTransaction")]
        [DataScoped(HasChanges = true, UseTransaction = true)]
        public IActionResult DataScopedWithChangesAndTransaction()
        {
            return new ObjectResult(GetDataScopedStatus());
        }

        private IEnumerable<FooModel> CreateList(String name, int count, bool createBar)
        {
            List<FooModel> models = new List<FooModel>();
            for (var i = 1; i < count; i++)
            {
                var foo = new FooModel {Name = $"{name} - {count}"};
                if (createBar)
                {
                    foo.Bar = new BarModel {Info = foo.Name};
                }
                models.Add(foo);
            }
            return models;
        }
    }
}
