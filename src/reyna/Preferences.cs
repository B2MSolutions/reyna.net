namespace Reyna
{
    using System;
    using Microsoft.Win32;

    internal static class Preferences
    {
        private const string StorageSizeLimitKeyName = "StorageSizeLimit";

        internal static long StorageSizeLimit
        {
            get
            {
                return GetRegistryValue(StorageSizeLimitKeyName, -1);
            }
        }

        internal static int ForwardServiceTemporaryErrorBackout
        {
            get
            {
                return GetRegistryValue("TemporaryErrorBackout", 5 * 60 * 1000);
            }
        }

        internal static int ForwardServiceMessageBackout
        {
            get
            {
                return GetRegistryValue("MessageBackout", 1000);
            }
        }

        internal static TimeRange CellularDataBlackout
        {
            get
            {
                try
                {
                    int minuteOfDayFrom = GetRegistryValue("DataBlackou:From", -1);
                    int minuteOfDayTo = GetRegistryValue("DataBlackout:To", -1);
                    if (minuteOfDayFrom == -1 || minuteOfDayTo == -1)
                    {
                        return null;
                    }

                    Time from = new Time(minuteOfDayFrom);
                    Time to = new Time(minuteOfDayTo);
                    return new TimeRange(from, to);
                }
                catch (Exception)
                {
                }

                return null;
            }
        }

        internal static void SetStorageSizeLimit(long limit)
        {
            SetRegistryValue(StorageSizeLimitKeyName, limit);
        }
        
        internal static void ResetStorageSizeLimit()
        {
            DeleteRegistryValue(StorageSizeLimitKeyName);
        }

        internal static void SetCellularDataBlackout(TimeRange range)
        {
            SetRegistryValue("DataBlackou:From", range.From.MinuteOfDay);
            SetRegistryValue("DataBlackout:To", range.To.MinuteOfDay);
        }

        internal static void ResetCellularDataBlackout()
        {
            DeleteRegistryValue("DataBlackou:From");
            DeleteRegistryValue("DataBlackout:To");
        }

        private static int GetRegistryValue(string keyName, int defaultValue)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Reyna", false))
            {
                if (key == null)
                {
                    return defaultValue;
                }

                return Convert.ToInt32(key.GetValue(keyName, defaultValue));
            }
        }

        private static void SetRegistryValue(string keyName, long value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Reyna"))
            {
                if (key != null)
                {
                    key.SetValue(keyName, value);
                }
            }
        }

        private static void DeleteRegistryValue(string keyName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Reyna", true))
            {
                if (key != null)
                {
                    key.DeleteValue(keyName, false);
                }
            }
        }
    }
}
