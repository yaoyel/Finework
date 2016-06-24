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
    public class TaskViewModel
    {
         public Guid Id { get; set; }

        public string Name { get; set; }

        public IList<string> Collabrator { get; set; }

        public IList<string> Leader { get; set; }

        public IList<string> Mentor { get; set; }

        public IList<string> Recipient { get; set; } 
    }
}