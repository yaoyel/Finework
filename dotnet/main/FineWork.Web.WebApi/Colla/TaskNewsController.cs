using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Text.RegularExpressions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/taskNews")]
    [Authorize("Bearer")]
    public class TaskNewsController : FwApiController
    {
        public TaskNewsController(ISessionProvider<AefSession> sessionProvider,
            ITaskNewsManager taskNewsManager)
            : base(sessionProvider)
        {
            if (taskNewsManager == null) throw new ArgumentNullException(nameof(taskNewsManager));
            m_TaskNewsManager = taskNewsManager;
        }
         
        private readonly ITaskNewsManager m_TaskNewsManager;

        [HttpPost("CreateTaskNews")]
        //[DataScoped(true)]
        public TaskNewsViewModel CreateTaskNews([FromBody] CreateTaskNewsModel newsModel)
        {
            using (var tx = TxManager.Acquire())
            {
                if (newsModel == null) throw new ArgumentNullException(nameof(newsModel));

                var news = m_TaskNewsManager.CreateTaskNews(newsModel);
                var result= news.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FetchTaskNewsesByTaskId")]
        public IEnumerable<TaskNewsViewModel> FetchTaskNewsesByTaskId(Guid taskId)
        {
            return m_TaskNewsManager.FetchTaskNewsesByTaskId(taskId)
                .Select(p => p.ToViewModel()).ToList();
        }

        [HttpGet("FetchUnReadNewsesByStaffId")]
        public Tuple<int,IEnumerable<Guid>> FetchUnReadNewsesByStaffId(Guid staffId)
        {
            return m_TaskNewsManager.FetchUnReadNewsesByStaffId(staffId);
        }

        [HttpGet("FetchNewsesByStaffId")]
        public IEnumerable<TaskNewsViewModel> FetchNewsesByStaffId(Guid staffId)
        {
            return m_TaskNewsManager.FetchTaskNewsesByStaffId(staffId)
                .Select(p => p.ToViewModel()).ToList();
        } 

        [HttpPost("DeleteNews")]
        public void DeleteNews(Guid newsId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_TaskNewsManager.DeleteTaskNewsById(newsId);
                tx.Complete(); 
            }
        }
    }

}
