namespace Reyna
{
    using System;
    using Microsoft.Win32;

    public class Registry : IRegistry
    {
        public string GetString(RegistryKey hive, string key, string valueName, string defaultValue)
        {
            using (var subKey = hive.OpenSubKey(key))
            {
                if (subKey == null)
                {
                    return defaultValue;
                }

                return subKey.GetValue(valueName, defaultValue) as string;
            }
        }

        public void SetString(RegistryKey hive, string key, string valueName, string value)
        {
            using (var subKey = hive.CreateSubKey(key))
            {
                subKey.SetValue(valueName, value);
            }
        }

        public long GetQWord(RegistryKey hive, string key, string valueName, long defaultValue)
        {
            using (var subKey = hive.OpenSubKey(key))
            {
                if (subKey == null)
                {
                    return defaultValue;
                }

                return (long)subKey.GetValue(valueName, defaultValue);
            }
        }

        public void SetQWord(RegistryKey hive, string key, string valueName, long value)
        {
            using (var subKey = hive.CreateSubKey(key))
            {
                subKey.SetValue(valueName, value, RegistryValueKind.QWord);
            }
        }

        public int GetDWord(RegistryKey hive, string key, string valueName, int defaultValue)
        {
            using (var subKey = hive.OpenSubKey(key))
            {
                if (subKey == null)
                {
                    return defaultValue;
                }

                return (int)subKey.GetValue(valueName, defaultValue);
            }
        }

        public void SetDWord(RegistryKey hive, string key, string valueName, int value)
        {
            using (var subKey = hive.CreateSubKey(key))
            {
                subKey.SetValue(valueName, value, RegistryValueKind.DWord);
            }
        }

        public void DeleteValue(RegistryKey hive, string key, string valueName)
        {
            using (var subKey = hive.OpenSubKey(key, true))
            {
                if (subKey == null)
                {
                    return;
                }

                subKey.DeleteValue(valueName, false);
            }
        }
    }
}
