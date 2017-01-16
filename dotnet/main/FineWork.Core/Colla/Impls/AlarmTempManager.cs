using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class AlarmTempManagerr : AefEntityManager<AlarmTempEntity, Guid>, IAlarmTempManager
    {
        public AlarmTempManagerr(ISessionProvider<AefSession> sessionProvider,IFileManager fileManager) 
            : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentException(nameof(sessionProvider));
            m_FileManager = fileManager;
        }

        private readonly IFileManager m_FileManager;
        public AlarmTempEntity CreateAlarmTemp(CreateAlarmPeriodModel model)
        {
            if (model.ShortTime == null)
                throw new ArgumentException(nameof(model.ShortTime)); 

            var alarmPeriod = new AlarmTempEntity();
            alarmPeriod.Id = Guid.NewGuid();
            if(model.Weekdays.HasValue)
            alarmPeriod.Weekdays = model.Weekdays.Value;
            alarmPeriod.Content = model.Content;
            alarmPeriod.ShortTime = model.ShortTime;
            alarmPeriod.Task = null;
            alarmPeriod.Bell = model.Bell;
            alarmPeriod.IsEnabled = model.IsEnabled;
            alarmPeriod.IsRepeat = model.IsRepeat;
            alarmPeriod.DaysInMonth = model.DaysInMonth;
            alarmPeriod.NoRepeatTime = model.NoRepeatTime;
            alarmPeriod.TempletKind = model.TempletKind;

            if (model.ReceiverStaffIds != null && model.ReceiverStaffIds.Any())
            {
                alarmPeriod.ReceiverStaffIds = string.Join(",", model.ReceiverStaffIds);
            }
            else
            {
                if (model.ReceiverKinds != null && model.ReceiverKinds.Any())
                    alarmPeriod.ReceiverKinds = string.Join(",", model.ReceiverKinds.OrderBy(p => p).Select(p => (int)p));
            }

            this.InternalInsert(alarmPeriod);

            return alarmPeriod;
        }

        public void DeleteAlarmTemps(params Guid[] alarmTempIds)
        {
            if(!alarmTempIds.Any()) return;
            foreach (var alarmTempId in alarmTempIds)
            {
                var temp = AlarmTempExistsResult.Check(this, alarmTempId).AlarmPeriod;
                if (temp != null)
                {
                    this.InternalDelete(temp);
                    m_FileManager.DeleteBlobDirectory(GetAlarmAttDirectory(temp));
                }
                   
            }
        }

        public void UpdateAlarmTemp(UpdateAlarmPeriodModel model)
        {
            var alarmPeriod = AlarmTempExistsResult.Check(this, model.AlarmId).ThrowIfFailed().AlarmPeriod;

            alarmPeriod.ShortTime = model.ShortTime;
            if(model.Weekdays.HasValue)
            alarmPeriod.Weekdays = model.Weekdays.Value;
            alarmPeriod.Bell = model.Bell;
            alarmPeriod.Content = model.Content;
            alarmPeriod.DaysInMonth = model.DaysInMonth;
            alarmPeriod.NoRepeatTime = model.NoRepeatTime;
            alarmPeriod.IsRepeat = model.IsRepeat;
            alarmPeriod.TempletKind = model.TempletKind;

            //更新接受者 
            if (model.ReceiverStaffIds != null && model.ReceiverStaffIds.Any())
            {
                alarmPeriod.ReceiverStaffIds = string.Join(",", model.ReceiverStaffIds);
            }
            else
            {
                if (model.ReceiverKinds != null && model.ReceiverKinds.Any())
                    alarmPeriod.ReceiverKinds = string.Join(",", model.ReceiverKinds.OrderBy(p => p).Select(p => (int)p));
            }
            this.InternalUpdate(alarmPeriod); 
        }

        public AlarmTempEntity FindById(Guid alarmTmepId)
        {
            return this.InternalFind(alarmTmepId);
        }

        public void UpdateAlarmTempEnabled(Guid alarmTempId, bool isEnabled)
        {
            var alarmTemp = AlarmTempExistsResult.Check(this, alarmTempId).ThrowIfFailed().AlarmPeriod;
            alarmTemp.IsEnabled = isEnabled;
            this.InternalUpdate(alarmTemp);

        }

        public IEnumerable<AlarmTempEntity> FetchOverdueAlarmTemps()
        {
            return this.InternalFetchAll().Where(p => DateTime.Now.AddDays(-1) > p.CreatedAt);
        }

        private string GetAlarmAttDirectory(AlarmEntity alarm)
        {
            return $"tasks/alarms/{alarm.Id}";
        }
    }
}