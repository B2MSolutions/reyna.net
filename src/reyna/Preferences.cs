namespace Reyna
{
    using System;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    public class Preferences
    {
        private const string SubKey = @"Software\Reyna";
        private const string StorageSizeLimitKeyName = "StorageSizeLimit";
        private const string WlanBlackoutRangeKeyName = "WlanBlackoutRange";
        private const string WwanBlackoutRangeKeyName = "WwanBlackoutRange";
        private const string RoamingBlackoutKeyName = "RoamingBlackout";
        private const string OnChargeBlackoutKeyName = "OnChargeBlackout";
        private const string OffChargeBlackoutKeyName = "OffChargeBlackout";

        public TimeRange CellularDataBlackout
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

        public string WlanBlackoutRange
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

        public string WwanBlackoutRange
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

        public bool RoamingBlackout
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

        public bool OnChargeBlackout
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

        public bool OffChargeBlackout
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

        internal long StorageSizeLimit
        {
            get
            {
                return GetRegistryValue(StorageSizeLimitKeyName, -1);
            }
        }

        public void SetCellularDataBlackout(TimeRange range)
        {
            SetRegistryValue("DataBlackou:From", range.From.MinuteOfDay);
            SetRegistryValue("DataBlackout:To", range.To.MinuteOfDay);
        }

        public void ResetCellularDataBlackout()
        {
            DeleteRegistryValue("DataBlackou:From");
            DeleteRegistryValue("DataBlackout:To");
        }

        public void SetWlanBlackoutRange(string range)
        {
            if (IsBlackoutRangeValid(range))
            {
                SetRegistryValue(WlanBlackoutRangeKeyName, range);
            }
            else
            {
                this.ResetWlanBlackoutRange();
            }
        }

        public void ResetWlanBlackoutRange()
        {
            DeleteRegistryValue(WlanBlackoutRangeKeyName);
        }

        public void SetWwanBlackoutRange(string range)
        {
            if (IsBlackoutRangeValid(range))
            {
                SetRegistryValue(WwanBlackoutRangeKeyName, range);
            }
            else
            {
                this.ResetWwanBlackoutRange();
            }
        }

        public void ResetWwanBlackoutRange()
        {
            DeleteRegistryValue(WwanBlackoutRangeKeyName);
        }

        public void SetRoamingBlackout(bool value)
        {
            SetRegistryValue(RoamingBlackoutKeyName, value);
        }

        public void ResetRoamingBlackout()
        {
            DeleteRegistryValue(RoamingBlackoutKeyName);
        }

        public void SetOnChargeBlackout(bool value)
        {
            SetRegistryValue(OnChargeBlackoutKeyName, value);
        }

        public void ResetOnChargeBlackout()
        {
            DeleteRegistryValue(OnChargeBlackoutKeyName);
        }

        public void SetOffChargeBlackout(bool value)
        {
            SetRegistryValue(OffChargeBlackoutKeyName, value);
        }

        public void ResetOffChargeBlackout()
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

        internal void SetStorageSizeLimit(long limit)
        {
            SetRegistryValue(StorageSizeLimitKeyName, limit);
        }

        internal void ResetStorageSizeLimit()
        {
            DeleteRegistryValue(StorageSizeLimitKeyName);
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
