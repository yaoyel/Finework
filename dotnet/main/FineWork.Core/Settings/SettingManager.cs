using System;
using FineWork.Settings.Repos;

namespace FineWork.Settings
{
    public class SettingManager : ISettingManager
    {
        public SettingManager(ISettingRepository settingRepository)
        {
            if (settingRepository == null) throw new ArgumentNullException("settingRepository");
            this.SettingRepository = settingRepository;
        }

        protected ISettingRepository SettingRepository { get; private set; }

        public bool Contains(String id)
        {
            ISetting item = this.SettingRepository.Find(id);
            return item != null;
        }

        public String GetValue(String id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");
            ISetting item = this.SettingRepository.Find(id);
            if (item != null)
            {
                return item.Value;
            }
            return null;
        }

        public void SetValue(String id, String value)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");
            ISetting item = this.SettingRepository.Find(id);
            if (item == null)
            {
                item = this.SettingRepository.Create(id);
                item.Value = value;
                this.SettingRepository.Insert(item);
            }
            else
            {
                item.Value = value;
                this.SettingRepository.Update(item);
            }
        }

        public void DeleteValue(String id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");
            ISetting item = this.SettingRepository.Find(id);
            if (item != null)
            {
                this.SettingRepository.Delete(item);
            }
        }
    }
}
