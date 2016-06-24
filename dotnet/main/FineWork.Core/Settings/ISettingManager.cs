using System;

namespace FineWork.Settings
{
    public interface ISettingManager
    {
        String GetValue(String id);

        void SetValue(String id, String value);

        void DeleteValue(String id);
    }
}