using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class TaskNewsExistsResult : FineWorkCheckResult
    {
        public TaskNewsExistsResult(bool isSucceed, string message, TaskNewsEntity taskNews)
            :base(isSucceed,message)
        {
            this.TaskNews = taskNews;
        }

        public TaskNewsEntity TaskNews { get; private set; }

        public static TaskNewsExistsResult Check(ITaskNewsManager  taskNewsManager, Guid taskNewsId)
        {
            if (taskNewsManager == null) throw new ArgumentException(nameof(taskNewsManager));

            var taskNews = taskNewsManager.FindTaskNewsById(taskNewsId);
            return Check(taskNews, "任务不存在对应的好消息.");
        }

        private static TaskNewsExistsResult Check(TaskNewsEntity taskNews, string message)
        {
            if (taskNews == null)
                return new TaskNewsExistsResult(false, message, null);
            return new TaskNewsExistsResult(true, null, taskNews);
        }
    }
}
