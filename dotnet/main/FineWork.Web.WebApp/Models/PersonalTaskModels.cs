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
    public class PersonalTaskViewModel
    {
        public IList<TaskViewModel> Collabrator { get; set; }

        public IList<TaskViewModel> Leader { get; set; }

        public IList<TaskViewModel> Mentor { get; set; }

        public IList<TaskViewModel> Recipient { get; set; }
    }

 

}