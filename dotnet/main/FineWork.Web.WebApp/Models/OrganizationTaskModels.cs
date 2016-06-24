using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Impls;
using FineWork.Web.WebApp.Models;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApp.Models
{
    public class OrganizationTaskViewModel
    {
        public Guid TaskId { get; set; }

         public  string TaskName { get; set; }
         
        public IList<string> Leader { get; set; } 
    }
}