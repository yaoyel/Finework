using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using JetBrains.Annotations; 
using System.Data.Entity;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class TaskIncentiveManager : AefEntityManager<TaskIncentiveEntity, Guid>, ITaskIncentiveManager
    {
        public TaskIncentiveManager(ISessionProvider<AefSession> dbContextProvider,
            ITaskManager taskManager, IIncentiveKindManager   incentiveKindManager  )
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (incentiveKindManager == null) throw new ArgumentNullException(nameof(incentiveKindManager)); 

            m_TaskManager = taskManager;
            m_IncentiveKindManager = incentiveKindManager;
            
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;


         public TaskIncentiveEntity FindTaskIncentiveById(Guid taskIncentiveId)
        {
            return this.InternalFind(taskIncentiveId);
        }

        /// <summary>
        /// 存在则更新，不存在则添加
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="kindId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public TaskIncentiveEntity UpdateTaskIncentive(Guid taskId, int kindId, decimal amount)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var taskIncentive = TaskIncentiveExistsResult.Check(this, taskId, kindId).TaskIncentive;
            var incentiveKind =
                IncentiveKindExistsResult.Check(this.m_IncentiveKindManager, kindId).ThrowIfFailed().IncentiveKind;

            if (taskIncentive == null)
                return this.InternalCreate(task, incentiveKind, amount);

            taskIncentive.Amount = amount;
            this.InternalUpdate(taskIncentive);
            return taskIncentive;
        }

        /// <summary>
        /// 默认不存在的情况返回值数量为0
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="kindId"></param>
        /// <param name="returnNull"></param>
        /// <returns></returns>
        public TaskIncentiveEntity FindTaskIncentiveByTaskIdAndKindId(Guid taskId, int kindId, bool returnNull = false)
        {
            var taskIncentive =
                this.InternalFetch(q => q.Where(p => p.Task.Id == taskId && p.IncentiveKind.Id == kindId)
                    .Include(t => t.Task)).SingleOrDefault();

            if (taskIncentive == null)
            {
                if (returnNull) return null;

                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).Task;
                var incentiveKind = IncentiveKindExistsResult.Check(this.m_IncentiveKindManager, kindId).IncentiveKind;
                return new TaskIncentiveEntity()
                {
                    Amount = 0,
                    Task = task,
                    IncentiveKind = incentiveKind
                };
            }

            return taskIncentive;
        }

        /// <summary>
        /// 返回任务激励，返回值是否包含未设置的kind,
        /// 默认为true
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="includeAllKind">是否返回所有的激励类型，未设置的激励值为0</param>
        /// <returns></returns>
        public IEnumerable<TaskIncentiveEntity> FetchTaskIncentiveByTaskId(Guid taskId, bool includeAllKind = true)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).Task;

            var taskIncentives = this.InternalFetch(p => p.Task.Id == taskId);
            if (!includeAllKind) return taskIncentives;

            return taskIncentives.Union(m_IncentiveKindManager.FetchIncentiveKind()
                .Select(p => new TaskIncentiveEntity()
                {
                    Amount = 0,
                    Task = task,
                    IncentiveKind = p
                }))
                .OrderByDescending(p => p.Amount)
                .GroupBy(p => p.IncentiveKind.Id)
                .Select(p => p.First());
        }

        public void DeleteTaskIncentiveByTaskId(Guid taskId)
        {
              var taskIncentives = this.InternalFetch(p => p.Task.Id == taskId);

            if(taskIncentives.Any())
                taskIncentives.ToList().ForEach(this.InternalDelete);
        }

        private TaskIncentiveEntity InternalCreate(TaskEntity task, IncentiveKindEntity incentiveKind, decimal amount)
        {
            var taskIncentive = new TaskIncentiveEntity()
            {
                Id = Guid.NewGuid(),
                Task = task,
                IncentiveKind = incentiveKind,
                Amount = amount
            };

            this.InternalInsert(taskIncentive);
            return taskIncentive;
        }

    }
}