namespace Reyna
{
    using Microsoft.Win32;

    public interface IRegistry
    {
        string GetString(RegistryKey hive, string key, string valueName, string defaultValue);

        long GetQWord(RegistryKey hive, string key, string valueName, long defaultValue);

        int GetDWord(RegistryKey hive, string key, string valueName, int defaultValue);

        void SetString(RegistryKey hive, string key, string valueName, string value);

        void SetQWord(RegistryKey hive, string key, string valueName, long value);

        void SetDWord(RegistryKey hive, string key, string valueName, int value);

        void DeleteValue(RegistryKey hive, string key, string valueName);
    }
}
