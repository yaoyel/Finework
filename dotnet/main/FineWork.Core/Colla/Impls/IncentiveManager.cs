using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Common;

namespace FineWork.Colla.Impls
{
    public class IncentiveManager : AefEntityManager<IncentiveEntity, Guid>, IIncentiveManager
    {
        public IncentiveManager(ISessionProvider<AefSession> dbContextProvider,
            IStaffManager staffManager, ITaskIncentiveManager taskIncentiveManager,
            ITaskLogManager taskLogManager)
            : base(dbContextProvider)
        {

            m_StaffManager = staffManager;
            m_TaskIncentiveManager = taskIncentiveManager;
            m_TaskLogManager = taskLogManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly ITaskLogManager m_TaskLogManager;

        public IncentiveEntity CreateIncentive(Guid taskId, int incentiveKindId, Guid senderStaffId,
            Guid receiverStaffId, decimal quantity)
        {
            var sender = StaffExistsResult.Check(this.m_StaffManager, senderStaffId).ThrowIfFailed().Staff;
            var receiver = StaffExistsResult.Check(this.m_StaffManager, receiverStaffId).ThrowIfFailed().Staff;
            var taskIncentive =
                TaskIncentiveExistsResult.Check(this.m_TaskIncentiveManager, taskId, incentiveKindId)
                    .TaskIncentive;

            if (taskIncentive == null)
                throw new FineWorkException("请先对任务的激励进行设置。");

            //判断余额
            //对应激励种类已经发出的额度
            var outQuantitys =
                this.InternalFetch(
                    p => p.TaskIncentive.Task.Id == taskId && p.TaskIncentive.IncentiveKind.Id == incentiveKindId)
                    .Sum(p => p.Quantity);
                  

            if (taskIncentive.Amount < (outQuantitys+quantity))
                throw new FineWorkException("余额不足，请查看任务设置。");


            var incentive = new IncentiveEntity()
            {
                Id = Guid.NewGuid(),
                SenderStaff = sender,
                ReceiverStaff = receiver,
                TaskIncentive = taskIncentive,
                Quantity = quantity
            };

            this.InternalInsert(incentive);


            //记入任务日志
            var message = $"发给{receiver.Name}一个{taskIncentive.IncentiveKind.Name}";
            m_TaskLogManager.CreateTaskLog(taskId, sender.Id, incentive.GetType().FullName, incentive.Id, ActionKinds.InsertTable,
                message);

            return incentive;
        }

        public IEnumerable<IncentiveEntity> FetchIncentiveByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.TaskIncentive.Task.Id == taskId
                                           && p.SenderStaff.IsEnabled);

        }
    }
}
