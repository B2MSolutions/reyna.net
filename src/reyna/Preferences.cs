namespace Reyna
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    internal static class Preferences
    {
        private const string SubKey = @"Software\Reyna";
        private const string StorageSizeLimitKeyName = "StorageSizeLimit";
        private const string WlanBlackoutRangeKeyName = "WlanBlackoutRange";
        private const string WwanBlackoutRangeKeyName = "WwanBlackoutRange";
        private const string RoamingBlackoutKeyName = "RoamingBlackout";
        private const string OnChargeBlackoutKeyName = "OnChargeBlackout";
        private const string OffChargeBlackoutKeyName = "OffChargeBlackout";

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

        internal static string WlanBlackoutRange
        {
            get
            {
                try
                {
                    return (string)GetRegistryValue(WlanBlackoutRangeKeyName, null);
                }
                catch (Exception)
                {
                }

                return null;
            }
        }

        internal static string WwanBlackoutRange
        {
            get
            {
                try
                {
                    return (string)GetRegistryValue(WwanBlackoutRangeKeyName, null);
                }
                catch (Exception)
                {
                }

                return null;
            }
        }

        internal static bool RoamingBlackout
        {
            get
            {
                try
                {
                    return GetRegistryValue(RoamingBlackoutKeyName, false);                    
                }
                catch (Exception)
                {
                }

                return true;
            }
        }

        internal static bool OnChargeBlackout
        {
            get
            {
                try
                {
                    return GetRegistryValue(OnChargeBlackoutKeyName, true);
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        internal static bool OffChargeBlackout
        {
            get
            {
                try
                {
                    return GetRegistryValue(OffChargeBlackoutKeyName, true);
                }
                catch (Exception)
                {
                }

                return false;
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

        internal static void SetWlanBlackoutRange(string range)
        {
            if (IsBlackoutRangeValid(range))
            {
                SetRegistryValue(WlanBlackoutRangeKeyName, range);
            }
            else
            {
                ResetWlanBlackoutRange();
            }
        }

        internal static void ResetWlanBlackoutRange()
        {
            DeleteRegistryValue(WlanBlackoutRangeKeyName);
        }

        internal static void SetWwanBlackoutRange(string range)
        {
            if (IsBlackoutRangeValid(range))
            {
                SetRegistryValue(WwanBlackoutRangeKeyName, range);
            }
            else
            {
                ResetWwanBlackoutRange();
            }
        }

        internal static void ResetWwanBlackoutRange()
        {
            DeleteRegistryValue(WwanBlackoutRangeKeyName);
        }

        internal static void SetRoamingBlackout(bool value)
        {
            SetRegistryValue(RoamingBlackoutKeyName, value);
        }

        internal static void ResetRoamingBlackout()
        {
            DeleteRegistryValue(RoamingBlackoutKeyName);
        }

        internal static void SetOnChargeBlackout(bool value)
        {
            SetRegistryValue(OnChargeBlackoutKeyName, value);
        }

        internal static void ResetOnChargeBlackout()
        {
            DeleteRegistryValue(OnChargeBlackoutKeyName);
        }

        internal static void SetOffChargeBlackout(bool value)
        {
            SetRegistryValue(OffChargeBlackoutKeyName, value);
        }

        internal static void ResetOffChargeBlackout()
        {
            DeleteRegistryValue(OffChargeBlackoutKeyName);
        }

        internal static bool IsBlackoutRangeValid(string ranges)
        {
            string[] splitRanges = ranges.Split(',');
            foreach (string range in splitRanges) 
            {
                string regex = "^[0-9][0-9]:[0-9][0-9]-[0-9][0-9]:[0-9][0-9]$";
                if (!Regex.IsMatch(range, regex, RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static int GetRegistryValue(string keyName, int defaultValue)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(SubKey, false))
            {
                if (key == null)
                {
                    return defaultValue;
                }

                return Convert.ToInt32(key.GetValue(keyName, defaultValue));
            }
        }

        private static object GetRegistryValue(string keyName, object defaultValue)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(SubKey, false))
            {
                if (key == null)
                {
                    return defaultValue;
                }

                return key.GetValue(keyName, defaultValue);
            }
        }

        private static bool GetRegistryValue(string keyName, bool defaultValue)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(SubKey, false))
            {
                if (key == null)
                {
                    return defaultValue;
                }

                return bool.Parse((string)key.GetValue(keyName, defaultValue));
            }
        }

        private static void SetRegistryValue(string keyName, long value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(SubKey))
            {
                if (key != null)
                {
                    key.SetValue(keyName, value);
                }
            }
        }

        private static void SetRegistryValue(string keyName, object value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(SubKey))
            {
                if (key != null)
                {
                    key.SetValue(keyName, value);
                }
            }
        }

        private static void DeleteRegistryValue(string keyName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(SubKey, true))
            {
                if (key != null)
                {
                    key.DeleteValue(keyName, false);
                }
            }
        }
    }
}
