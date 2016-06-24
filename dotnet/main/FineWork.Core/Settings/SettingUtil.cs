using System;
using System.ComponentModel;

namespace FineWork.Settings
{
    public static class SettingUtil
    {
        public static T GetAs<T>(ISettingManager settingManager, String id, T defaultValue)
        {
            if (settingManager == null) throw new ArgumentNullException("settingManager");
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");

            String value = settingManager.GetValue(id);

            if (value != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof (T));
                Object rawValue = converter.ConvertFromString(value);
                return (T) rawValue;
            }

            return defaultValue;
        }

        public static T SetAs<T>(ISettingManager settingManager, String id, T value)
        {
            if (settingManager == null) throw new ArgumentNullException("settingManager");
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");

            T oldValue = GetAs(settingManager, id, default(T));

            TypeConverter converter = TypeDescriptor.GetConverter(typeof (T));
            string s = converter.ConvertToString(value);
            settingManager.SetValue(id, s);

            return oldValue;
        }
    }
}