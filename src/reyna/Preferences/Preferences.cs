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
        private const string DataBlackoutFromKeyName = "DataBlackout:From";
        private const string DataBlackoutToKeyName = "DataBlackout:To";
        private const string TemporaryErrorBackout = "TemporaryErrorBackout";
        private const string MessageBackout = "MessageBackout";

        public TimeRange CellularDataBlackout
        {
            get
            {
                try
                {
                    int minuteOfDayFrom = GetRegistryValue(DataBlackoutFromKeyName, -1);
                    int minuteOfDayTo = GetRegistryValue(DataBlackoutToKeyName, -1);
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
                return GetRegistryValue(WlanBlackoutRangeKeyName, null);
            }
        }

        public string WwanBlackoutRange
        {
            get
            {
                return GetRegistryValue(WwanBlackoutRangeKeyName, null);
            }
        }

        public bool RoamingBlackout
        {
            get
            {
                return GetRegistryValue(RoamingBlackoutKeyName, true);
            }
        }

        public bool OnChargeBlackout
        {
            get
            {
                return GetRegistryValue(OnChargeBlackoutKeyName, false);
            }
        }

        public bool OffChargeBlackout
        {
            get
            {
                return GetRegistryValue(OffChargeBlackoutKeyName, false);
            }
        }

        internal static int ForwardServiceTemporaryErrorBackout
        {
            get
            {
                return GetRegistryValue(TemporaryErrorBackout, 5 * 60 * 1000);
            }
        }

        internal static int ForwardServiceMessageBackout
        {
            get
            {
                return GetRegistryValue(MessageBackout, 1000);
            }
        }

        internal long StorageSizeLimit
        {
            get
            {
                return GetRegistryValue(StorageSizeLimitKeyName, (long)-1);
            }
        }

        public void SetCellularDataBlackout(TimeRange range)
        {
            SetRegistryValue(DataBlackoutFromKeyName, range.From.MinuteOfDay);
            SetRegistryValue(DataBlackoutToKeyName, range.To.MinuteOfDay);
        }

        public void ResetCellularDataBlackout()
        {
            DeleteRegistryValue(DataBlackoutFromKeyName);
            DeleteRegistryValue(DataBlackoutToKeyName);
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
            if (string.IsNullOrEmpty(ranges))
            {
                return false;
            }

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

        internal static void SaveCellularDataAsWwanForBackwardsCompatibility()
        {
            Preferences preferences = new Preferences();
            TimeRange timeRange = preferences.CellularDataBlackout;
            if (timeRange != null)
            {
                int hourFrom = (int)Math.Floor((double)timeRange.From.MinuteOfDay / 60);
                int minuteFrom = timeRange.From.MinuteOfDay % 60;
                string blackoutFrom = ZeroPad(hourFrom) + ":" + ZeroPad(minuteFrom);

                int hourTo = (int)Math.Floor((double)timeRange.To.MinuteOfDay / 60);
                int minuteTo = timeRange.To.MinuteOfDay % 60;
                string blackoutTo = ZeroPad(hourTo) + ":" + ZeroPad(minuteTo);

                preferences.SetWwanBlackoutRange(blackoutFrom + "-" + blackoutTo);
            }
        }

        internal void SetStorageSizeLimit(long limit)
        {
            SetRegistryValue(StorageSizeLimitKeyName, limit);
        }

        internal void ResetStorageSizeLimit()
        {
            DeleteRegistryValue(StorageSizeLimitKeyName);
        }

        private static long GetRegistryValue(string keyName, long defaultValue)
        {
            return new Registry().GetQWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, defaultValue);
        }

        private static int GetRegistryValue(string keyName, int defaultValue)
        {
            return new Registry().GetDWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, defaultValue);
        }

        private static bool GetRegistryValue(string keyName, bool defaultValue)
        {
            return new Registry().GetDWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, defaultValue ? 1 : 0) == 1;
        }

        private static string GetRegistryValue(string keyName, string defaultValue)
        {
            return new Registry().GetString(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, defaultValue);
        }

        private static void SetRegistryValue(string keyName, long value)
        {
            new Registry().SetQWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, value);
        }

        private static void SetRegistryValue(string keyName, string value)
        {
            new Registry().SetString(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, value);
        }

        private static void SetRegistryValue(string keyName, bool value)
        {
            new Registry().SetDWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, value ? 1 : 0);
        }

        private static void SetRegistryValue(string keyName, int value)
        {
            new Registry().SetDWord(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName, value);
        }

        private static void DeleteRegistryValue(string keyName)
        {
            new Registry().DeleteValue(Microsoft.Win32.Registry.LocalMachine, SubKey, keyName);
        }

        private static object ZeroPad(int numToPad)
        {
            return numToPad.ToString("D2");
        }
    }
}
