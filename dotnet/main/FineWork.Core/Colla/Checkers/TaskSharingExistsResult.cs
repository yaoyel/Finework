using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class TaskSharingExistsResult: FineWorkCheckResult
    {
        public TaskSharingExistsResult(bool isSucceed, String message, TaskSharingEntity tasksharing)
            : base(isSucceed, message)
        {
            this.TaskSharing = tasksharing;
        }

        public TaskSharingEntity TaskSharing { get; private set; }

        public static TaskSharingExistsResult Check(ITaskSharingManager taskSharingManager, Guid taskSharingId)
        {
            var taskSharing = taskSharingManager.FindTaskSharing(taskSharingId); 

            return Check(taskSharing, "不存在对应的任务共享信息.");
        }

        public static TaskSharingExistsResult Check(ITaskSharingManager taskSharingManager,Guid taskId, Guid taskSharingId)
        {
            var taskSharing = taskSharingManager.FindTaskSharingWithTask(taskId,taskSharingId);

            return Check(taskSharing, "不存在对应的任务共享信息.");
        }

        public static TaskSharingExistsResult Check(ITaskSharingManager taskSharingManager, string  contentMd5)
        {
            var taskSharing = taskSharingManager.FetchTaskSharingByContentMd5(contentMd5).FirstOrDefault();

            return Check(taskSharing, "不存在对应的任务共享信息.");
        }
         

        private static TaskSharingExistsResult Check([CanBeNull] TaskSharingEntity taskSharing, String message)
        {
            if (taskSharing == null)
            {
                return new TaskSharingExistsResult(false, message, null);
            }
            return new TaskSharingExistsResult(true, null, taskSharing);
        }
    }
} 