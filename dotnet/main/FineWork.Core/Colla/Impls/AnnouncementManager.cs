 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
 using FineWork.Core;
 using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class AnnouncementManager: AefEntityManager<AnnouncementEntity, Guid>, IAnnouncementManager
    {
        public AnnouncementManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IStaffManager staffManager,  
            IAnncAttManager anncAttManager, 
            IPartakerManager partakerManager,
            IIMService imService,
            ITaskLogManager taskLogManager,
            IConfiguration config,
            IAnncAlarmManager anncAlarmManager,
            IAnncAlarmRecManager anncAlarmRecManager, 
            ILazyResolver<IAnncExecutorManager> anncExectorManagerResolver,
            ILazyResolver<IAnncUpdateManager> anncUpdateResolver,
            ILazyResolver<ITaskSharingManager> taskSharingManagerResolver,
            ILazyResolver<IPushLogManager> pushLogManagerResolver ) : base(sessionProvider)
        {
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(staffManager, nameof(staffManager)); 
            Args.NotNull(anncAttManager, nameof(anncAttManager)); 
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(taskLogManager, nameof(taskLogManager));
            Args.NotNull(config, nameof(config));
            Args.NotNull(anncAlarmManager, nameof(anncAlarmManager));
            Args.NotNull(anncExectorManagerResolver, nameof(anncExectorManagerResolver));
            m_TaskManager = taskManager;
            m_StaffManager = staffManager; 
            m_AnncAttManager = anncAttManager; 
            m_PartakerManager = partakerManager;
            m_ImService = imService;
            m_TaskLogManager = taskLogManager;
            m_Config = config;
            m_AnncAlarmManager = anncAlarmManager;
            m_AnncAlarmRecManager = anncAlarmRecManager;
            m_AnncExectorManagerResolver = anncExectorManagerResolver;
            m_AnncUpdateResolver = anncUpdateResolver;
            m_TaskSharingManagerResolver = taskSharingManagerResolver;
            m_PushLogManageResolver = pushLogManagerResolver;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager; 
        private readonly IAnncAttManager m_AnncAttManager; 
        private readonly IPartakerManager m_PartakerManager;
        private readonly IIMService m_ImService;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IConfiguration m_Config;
        private readonly IAnncAlarmManager m_AnncAlarmManager;
        private readonly IAnncAlarmRecManager m_AnncAlarmRecManager;
        private readonly ILazyResolver<IAnncExecutorManager> m_AnncExectorManagerResolver;
        private readonly ILazyResolver<IAnncUpdateManager> m_AnncUpdateResolver;
        private readonly ILazyResolver<ITaskSharingManager> m_TaskSharingManagerResolver;
        private readonly ILazyResolver<IPushLogManager> m_PushLogManageResolver;
        private IAnncExecutorManager AnncExecutorManager {
            get { return m_AnncExectorManagerResolver.Required; }
        } 

        private IAnncUpdateManager AnncUpdateManager
        {
            get { return m_AnncUpdateResolver.Required; }
        } 

        private ITaskSharingManager TaskSharingManager
        {
            get { return m_TaskSharingManagerResolver.Required; }
        }

        private IPushLogManager PushLogManager
        { 
            get { return m_PushLogManageResolver.Required; }
        }

        public AnnouncementEntity CreateAnnc(CreateAnncModel createAnncModel, bool checkPartaker = true)
        {
            //存为草稿的时候不需要验证执行人和验收人
            if ((createAnncModel.ExecutorIds == null || !createAnncModel.ExecutorIds.Any()))
                throw new FineWorkException("请选择执行者");
            if (createAnncModel.InspecterId == default(Guid))
                throw new FineWorkException("请选择验收者");

            StaffEntity inspecter;
            var task = TaskExistsResult.Check(m_TaskManager, createAnncModel.TaskId).ThrowIfFailed().Task;
            var creator = StaffExistsResult.Check(m_StaffManager, createAnncModel.CreatorId).ThrowIfFailed().Staff;

            //复制计划时，不要求成员为任务成员，非任务成员添加后为协同者
            if (createAnncModel.InspecterId.HasValue)
                if (checkPartaker)
                    inspecter =
                        PartakerExistsResult.CheckForStaff(task, createAnncModel.InspecterId.Value)
                            .ThrowIfFailed()
                            .Partaker.Staff;
                else
                {
                    var partaker = PartakerExistsResult.CheckForStaff(task, createAnncModel.InspecterId.Value).Partaker;
                    if (partaker != null)
                        inspecter = partaker.Staff;
                    else
                    {
                        inspecter =
                            StaffExistsResult.Check(m_StaffManager, createAnncModel.InspecterId.Value)
                                .ThrowIfFailed()
                                .Staff;
                        m_PartakerManager.CreateCollabrator(task.Id, inspecter.Id);
                    }
                }
            else
                inspecter = null;

            var executors = new List<AnncExecutorEntity>();
            var anncEntity = new AnnouncementEntity();
            anncEntity.Id = Guid.NewGuid();
            anncEntity.Creator = creator;
            anncEntity.Task = task;
            anncEntity.Inspecter = inspecter;
            anncEntity.StartAt = createAnncModel.StartAt;
            anncEntity.Content = createAnncModel.Content;
            anncEntity.IsNeedAchv = createAnncModel.IsNeedAchv;
            anncEntity.EndAt = createAnncModel.EndAt;

            this.InternalInsert(anncEntity);

            //新版 执行人设置为多人
            if (createAnncModel.ExecutorIds != null && createAnncModel.ExecutorIds.Any())
            {
                foreach (var executorId in createAnncModel.ExecutorIds)
                {
                    executors.Add(AnncExecutorManager.CreateAnncExecutor(anncEntity.Id, executorId));
                }
            }

            //处理预警
            if (createAnncModel.Alarms != null && createAnncModel.Alarms.Any())
            {
                foreach (var alarm in createAnncModel.Alarms)
                {
                    alarm.AnncId = anncEntity.Id;
                    m_AnncAlarmManager.CreateAnnAlarm(alarm);
                }
            }

            //处理资源
            if (createAnncModel.Atts != null && createAnncModel.Atts.Any())
                foreach (var att in createAnncModel.Atts)
                {
                    m_AnncAttManager.CreateAnncAtt(anncEntity.Id, att, false);
                }

            var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
            var message = String.Format(m_Config["LeanCloud:Messages:Task:Annc"], creator.Name,
                string.Join(",", executors.Select(p => p.Staff.Name)));
            m_ImService.SendTextMessageByConversationAsync(task.Id, creator.Account.Id, task.Conversation.Id,
                task.Name, message);

            m_TaskLogManager.CreateTaskLog(task.Id, leader.Staff.Id, anncEntity.GetType().FullName, anncEntity.Id,
                ActionKinds.InsertTable, "创建了一个计划");

            return anncEntity;
        } 

        public IList<AnnouncementEntity> CreateAnncs(IList<CreateAnncModel> createAnncModel, bool checkPartaker = true)
        {
            var anncs = new List<AnnouncementEntity>();
            if (createAnncModel.Any())

                createAnncModel.ToList().ForEach(p =>
                { 
                    if (p.Atts != null && p.Atts.Any())
                    {
                        var atts =new List<Guid>();

                        for (int i = 0; i < p.Atts.Length;i++)
                        {
                            if (
                                TaskSharingExistsResult.Check(TaskSharingManager, p.TaskId, p.Atts[i]).TaskSharing ==
                                null)
                            {
                                var taskSharing = TaskSharingManager.CreateTaskSharing(p.TaskId, p.CreatorId,p.Atts[i]);
                                atts.Add(taskSharing.Id);
                            }
                            else
                                atts.Add(p.Atts[i]);
                        } 
                        p.Atts = atts.ToArray();
                    } 
                    anncs.Add(CreateAnnc(p, checkPartaker)); 
                });
            return anncs;
        } 

        public void CreateAnnc(AnnouncementEntity annc)
        {
            Args.NotNull(annc, nameof(annc));
            this.InternalInsert(annc); 
        }

        public void UpdateAnnc(UpdateAnncModel updateAnncModel)
        {
            Args.NotNull(updateAnncModel, nameof(updateAnncModel));

            if ((updateAnncModel.ExecutorIds == null || !updateAnncModel.ExecutorIds.Any()))
                throw new FineWorkException("请选择执行者");
            if (updateAnncModel.InspecterId == default(Guid))
                throw new FineWorkException("请选择验收者");

            var annc = AnncExistsResult.Check(this, updateAnncModel.Id).ThrowIfFailed().Annc;

            if (updateAnncModel.StaffId != default(Guid))
            {
                var updateStaff =
                    StaffExistsResult.Check(this.m_StaffManager, updateAnncModel.StaffId).ThrowIfFailed().Staff;

                if (updateStaff.Id != annc.Creator.Id && updateStaff.Id != annc.Inspecter.Id &&
                    annc.Executors.All(p => p.Staff.Id != updateStaff.Id))
                {
                    throw new FineWorkException("你没有权限修改此计划.");
                } 

                AnncUpdateManager.CreateAnncUpdate(annc.Id, updateAnncModel.StaffId);
            }

            var creator = StaffExistsResult.Check(m_StaffManager, updateAnncModel.CreatorId).ThrowIfFailed().Staff;
            var inspecter = StaffExistsResult.Check(m_StaffManager, updateAnncModel.InspecterId).ThrowIfFailed().Staff; 

            var originalInspecterId = annc.Inspecter.Id; 

            annc.Content = updateAnncModel.Content;
            annc.EndAt = updateAnncModel.EndAt;
            annc.IsNeedAchv = updateAnncModel.IsNeedAchv;
            annc.Inspecter = inspecter;
            annc.StartAt = updateAnncModel.StartAt;
            annc.Creator = creator;

            this.m_AnncAttManager.UpdateAnncAtts(annc, updateAnncModel.Atts);


            this.m_AnncAlarmManager.DeleteAnncAlarmByAnncId(annc.Id);

            if (updateAnncModel.Alarms != null && updateAnncModel.Alarms.Any())
            {
                foreach (var alarm in updateAnncModel.Alarms)
                {
                   if(alarm.AnncAlarmId.HasValue && AnncAlarmExistsResult.Check(this.m_AnncAlarmManager,alarm.AnncAlarmId.Value).AnncAlarm!=null)
                        m_AnncAlarmManager.UpdateAnncAlarm(alarm);
                    else
                    {
                        var createAnncAlarmModel = new CreateAnncAlarmModel();
                        createAnncAlarmModel.AnncId = updateAnncModel.Id;
                        createAnncAlarmModel.Bell = alarm.Bell;
                        createAnncAlarmModel.IsEnabled = alarm.IsEnabled;
                        createAnncAlarmModel.Time = alarm.Time;
                        createAnncAlarmModel.Recs = alarm.Recs;
                        createAnncAlarmModel.BeforeStart = alarm.BeforeStart;
                        m_AnncAlarmManager.CreateAnnAlarm(createAnncAlarmModel);
                    }
                }
            }

            //新版 执行者变成多人 
            var originalExectorIds = annc.Executors.Select(p => p.Staff.Id).ToArray();

            //执行者对应的预警
            var alarms =
                m_AnncAlarmManager.FetchAnncAlarmsByAnncId(annc.Id)
                    .Where(p => p.Recs.Any(a => a.AnncRole == AnncRoles.Excutor))
                    .ToList();
            var replaceAlarm =
                alarms.Where(p => !originalExectorIds.Except(p.Recs.Select(s => s.Staff.Id)).Any()).ToList();

            if (originalExectorIds.Except(updateAnncModel.ExecutorIds).Any() ||
                updateAnncModel.ExecutorIds.Except(originalExectorIds).Any())
            {
                this.AnncExecutorManager.DeleteAnncExecutorByAnncId(annc.Id);

                foreach (var staffId in updateAnncModel.ExecutorIds)
                {
                    this.AnncExecutorManager.CreateAnncExecutor(annc.Id, staffId);
                }

                if (updateAnncModel.IsCoverExcutor)
                {
                    //新增的执行者
                    var newExecutors = updateAnncModel.ExecutorIds.Except(originalExectorIds).ToArray(); 
                    if (newExecutors.Any() && replaceAlarm.Any())
                        foreach (var alarm in alarms)
                        {
                            foreach (var newExecutor in newExecutors)
                            {
                                m_AnncAlarmRecManager.CreateAnncAlarmRec(alarm.Id, newExecutor, AnncRoles.Excutor);
                            } 
                        } 
                }
                
                //删除的执行者
                var delExectors = originalExectorIds.Except(updateAnncModel.ExecutorIds).ToList();
               
                if (delExectors.Any() && replaceAlarm.Any())
                {
                    foreach (var alarm in replaceAlarm)
                    {
                        foreach (var delExector in delExectors)
                        {
                            m_AnncAlarmRecManager.DeleteRecByAnncId(alarm.Annc.Id, delExector, AnncRoles.Excutor);
                        }

                    }
                }
            }


            if (originalInspecterId != updateAnncModel.InspecterId)
                if (updateAnncModel.IsCoverInspecter)
                {
                    var recs =
                        m_AnncAlarmRecManager.FetchRecsByAnncIdWithStaffId(annc.Id, originalInspecterId, AnncRoles.Inspecter)
                            .ToList();
                    if (recs.Any())
                        foreach (var rec in recs)
                        {
                            rec.Staff = inspecter;
                            this.m_AnncAlarmRecManager.UpdateAnncAlarmRec(rec);
                        }
                }
                else
                {
                    m_AnncAlarmRecManager.DeleteRecByAnncId(annc.Id, originalInspecterId, AnncRoles.Inspecter);
                }

            this.InternalUpdate(annc); 

        }

        public AnnouncementEntity FindAnncById(Guid anncId)
        {
            return this.InternalFind(anncId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId).OrderBy(p=>p.EndAt);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Creator.Id == staffId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncByStatus(Guid staffId,ReviewStatuses status=ReviewStatuses.Unspecified)
        {
            //获取staff参与的任务
            var taskIds = this.m_PartakerManager.FetchPartakersByStaff(staffId).Select(p => p.Task.Id).ToArray(); 
            
            return
                this.InternalFetch(
                    p =>p.EndAt<=DateTime.Now && taskIds.Contains(p.Task.Id) && !p.Reviews.Any());
        } 

        public void DeleteAnnc(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this, anncId).Annc;
            if (annc != null)
            {
                this.m_AnncAlarmManager.DeleteAnncAlarmByAnncId(anncId);
                this.AnncExecutorManager.DeleteAnncExecutorByAnncId(anncId);
                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId,true);
                this.AnncUpdateManager.DeleteAnncUpdatesByAnncId(anncId);  
                this.PushLogManager.DeletePushLogsByAnnc(anncId);
                this.InternalDelete(annc);
            }
        }

        public void DeleteAnncsByTaskId(Guid taskId)
        {
            var anncs = this.InternalFetch(p => p.Task.Id == taskId).ToList();

            if(anncs.Any())
                anncs.ForEach(InternalDelete); 
        }
    }
}
