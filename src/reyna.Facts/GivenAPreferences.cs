namespace Reyna.Facts
{
    using Microsoft.Win32;
    using Xunit;

    public class GivenAPreferences
    {
        [Fact]
        public void WhenSettingCellularDataBlackoutThenGetCellularDataBlackoutShouldReturnExpected()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            Preferences.SetCellularDataBlackout(range);
            
            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Equal(range.From.MinuteOfDay, timeRange.From.MinuteOfDay);
            Assert.Equal(range.To.MinuteOfDay, timeRange.To.MinuteOfDay);
        }

        [Fact]
        public void WhenResetCellularDataBlackoutThenGetCellularDataBlackoutShouldReturnNull()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            Preferences.ResetCellularDataBlackout();

            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Null(timeRange);
        }

        [Fact]
        public void WhenGetCellularDataBlackoutAndNotCorrectlySavedShouldReturnNull()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            Preferences.SetCellularDataBlackout(range);
            DeleteRegistryValue("DataBlackou:From");

            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Null(timeRange);
        }

        [Fact]
        public void WhenSettingWlanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnExpected()
        {
            Preferences.SetWlanBlackoutRange("00:00-00:01");

            string actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01", actual);
            Assert.Equal("00:00-00:01", actual);

            Preferences.SetWlanBlackoutRange("00:00-00:01,01:00-01:30");

            actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01,01:00-01:30", actual);
            Assert.Equal("00:00-00:01,01:00-01:30", actual);

            Preferences.SetWlanBlackoutRange("00:00-00:01,01:00-01:30,11:23-18:20");

            actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01,01:00-01:30,11:23-18:20", actual);
            Assert.Equal("00:00-00:01,01:00-01:30,11:23-18:20", actual);
        }

        [Fact]
        public void WhenResetWlanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnNull()
        {
            string range = "00:00-00:01";
            Preferences.ResetWlanBlackoutRange();

            range = Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenGetWlanBlackoutRangeAndNotCorrectlySavedShouldReturnNull()
        {
            Preferences.SetWlanBlackoutRange("00:00-00:01");
            DeleteRegistryValue("WlanBlackoutRange");

            string range = Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingWwanBlackoutRangeThenGetWwanBlackoutRangeShouldReturnExpected()
        {
            Preferences.SetWwanBlackoutRange("00:00-00:01");

            string actual = Preferences.WwanBlackoutRange;

            Assert.Equal("00:00-00:01", actual);
            Assert.Equal("00:00-00:01", actual);
        }

        [Fact]
        public void WhenResetWwanBlackoutRangeThenGetWwanBlackoutRangeShouldReturnNull()
        {
            string range = "00:00-00:01";
            Preferences.ResetWwanBlackoutRange();

            range = Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenGetWwanBlackoutRangeAndNotCorrectlySavedShouldReturnNull()
        {
            Preferences.SetWwanBlackoutRange("00:00-00:01");
            DeleteRegistryValue("WwanBlackoutRange");

            string range = Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingRoamingBlackoutThenGetRoamingBlackoutShouldReturnExpected()
        {
            Preferences.SetRoamingBlackout(true);
            bool roamingBlackout = Preferences.RoamingBlackout;

            Assert.True(roamingBlackout);

            Preferences.SetRoamingBlackout(false);
            roamingBlackout = Preferences.RoamingBlackout;

            Assert.False(roamingBlackout);
        }

        [Fact]
        public void WhenResetRoamingBlackoutThenGetRoamingBlackoutShouldReturnFalse()
        {
            Preferences.SetRoamingBlackout(true);
            Preferences.ResetRoamingBlackout();
            bool roamingBlackout = Preferences.RoamingBlackout;

            Assert.False(roamingBlackout);
        }

        [Fact]
        public void WhenGetRoamingBlackoutAndNotCorrectlySavedReturnTrue()
        {
            Preferences.SetRoamingBlackout(true);
            DeleteRegistryValue("RoamingBlackout");
            bool roamingBlackout = Preferences.RoamingBlackout;

            Assert.False(roamingBlackout);
        }

        [Fact]
        public void WhenSettingOnChargeBlackoutThenGetOnChargeBlackoutShouldReturnExpected()
        {
            Preferences.SetOnChargeBlackout(true);
            bool chargingBlackout = Preferences.OnChargeBlackout;

            Assert.True(chargingBlackout);

            Preferences.SetOnChargeBlackout(false);
            chargingBlackout = Preferences.OnChargeBlackout;

            Assert.False(chargingBlackout);
        }

        [Fact]
        public void WhenResetOnChargeBlackoutThenGetOnChargeBlackoutShouldReturnTrue()
        {
            Preferences.SetOnChargeBlackout(false);
            Preferences.ResetOnChargeBlackout();
            bool chargingBlackout = Preferences.OnChargeBlackout;

            Assert.True(chargingBlackout);
        }

        [Fact]
        public void WhenGetOnChargeBlackoutAndNotCorrectlySavedReturnTrue()
        {
            Preferences.SetOnChargeBlackout(false);
            DeleteRegistryValue("OnChargeBlackout");
            bool chargingBlackout = Preferences.OnChargeBlackout;

            Assert.True(chargingBlackout);
        }

        [Fact]
        public void WhenSettingOffChargeBlackoutThenGetOffChargeBlackoutShouldReturnExpected()
        {
            Preferences.SetOffChargeBlackout(true);
            bool dischargingBlackout = Preferences.OffChargeBlackout;

            Assert.True(dischargingBlackout);

            Preferences.SetOffChargeBlackout(false);
            dischargingBlackout = Preferences.OffChargeBlackout;

            Assert.False(dischargingBlackout);
        }

        [Fact]
        public void WhenResetOffChargeBlackoutThenGetOffChargeBlackoutShouldReturnTrue()
        {
            Preferences.SetOffChargeBlackout(false);
            Preferences.ResetOffChargeBlackout();
            bool dischargingBlackout = Preferences.OffChargeBlackout;

            Assert.True(dischargingBlackout);
        }

        [Fact]
        public void WhenGetOffChargeBlackoutAndNotCorrectlySavedReturnTrue()
        {
            Preferences.SetOffChargeBlackout(false);
            DeleteRegistryValue("OffChargeBlackout");
            bool dischargingBlackout = Preferences.OffChargeBlackout;

            Assert.True(dischargingBlackout);
        }

        [Fact]
        public void IsBlackoutRangeValidShouldReturnExpected()
        {
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30"));
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30,03:30-06:00"));
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30,03:30-06:00,07:00-07:01"));

            Assert.False(Preferences.IsBlackoutRangeValid(string.Empty));
            Assert.False(Preferences.IsBlackoutRangeValid("00:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1:0002:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:10-"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:10-1"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:00-02:30-15:42"));
        }

        private static void DeleteRegistryValue(string keyName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Reyna", true))
            {
                if (key != null)
                {
                    key.DeleteValue(keyName);
                }
            }
        }
    }
}
