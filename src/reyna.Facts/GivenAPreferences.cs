namespace Reyna.Facts
{
    using Microsoft.Win32;
    using Xunit;

    public class GivenAPreferences
    {
        public GivenAPreferences()
        {
            this.Preferences = new Preferences();
        }

        public Preferences Preferences { get; set; }

        [Fact]
        public void WhenGettingCellularDataBlackoutAndThrowsShouldReturnNull()
        {
            var registery = new Reyna.Registry();
            registery.SetDWord(Registry.LocalMachine, @"Software\Reyna", "DataBlackout:From", -3);
            registery.SetDWord(Registry.LocalMachine, @"Software\Reyna", "DataBlackout:To", -3);
            var timeRange = Preferences.CellularDataBlackout;

            Assert.Null(timeRange);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenSettingCellularDataBlackoutThenGetCellularDataBlackoutShouldReturnExpected()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            this.Preferences.SetCellularDataBlackout(range);
            
            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Equal(range.From.MinuteOfDay, timeRange.From.MinuteOfDay);
            Assert.Equal(range.To.MinuteOfDay, timeRange.To.MinuteOfDay);
        }

        [Fact]
        public void WhenResetCellularDataBlackoutThenGetCellularDataBlackoutShouldReturnNull()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            this.Preferences.ResetCellularDataBlackout();

            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Null(timeRange);
        }

        [Fact]
        public void WhenGetCellularDataBlackoutAndNotCorrectlySavedShouldReturnNull()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            this.Preferences.SetCellularDataBlackout(range);
            DeleteRegistryValue("DataBlackout:From");

            TimeRange timeRange = Preferences.CellularDataBlackout;

            Assert.Null(timeRange);
        }

        [Fact]
        public void WhenSettingWlanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnExpected()
        {
            this.Preferences.SetWlanBlackoutRange("00:00-00:01");

            string actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01", actual);
            Assert.Equal("00:00-00:01", actual);

            this.Preferences.SetWlanBlackoutRange("00:00-00:01,01:00-01:30");

            actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01,01:00-01:30", actual);
            Assert.Equal("00:00-00:01,01:00-01:30", actual);

            this.Preferences.SetWlanBlackoutRange("00:00-00:01,01:00-01:30,11:23-18:20");

            actual = Preferences.WlanBlackoutRange;

            Assert.Equal("00:00-00:01,01:00-01:30,11:23-18:20", actual);
            Assert.Equal("00:00-00:01,01:00-01:30,11:23-18:20", actual);
        }

        [Fact]
        public void WhenResetWlanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnNull()
        {
            string range = "00:00-00:01";
            this.Preferences.ResetWlanBlackoutRange();

            range = this.Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingWlanBlackoutRangeWithInvalidRangeShouldResetIt()
        {
            this.Preferences.SetWlanBlackoutRange("00");

            var range = this.Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingWwanBlackoutRangeWithInvalidRangeShouldResetIt()
        {
            this.Preferences.SetWwanBlackoutRange("00");

            var range = this.Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenNoWlanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnNull()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var range = this.Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenNoWwanBlackoutRangeThenGetWlanBlackoutRangeShouldReturnNull()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var range = this.Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenGetWlanBlackoutRangeAndNotCorrectlySavedShouldReturnNull()
        {
            this.Preferences.SetWlanBlackoutRange("00:00-00:01");
            DeleteRegistryValue("WlanBlackoutRange");

            string range = this.Preferences.WlanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingWwanBlackoutRangeThenGetWwanBlackoutRangeShouldReturnExpected()
        {
            this.Preferences.SetWwanBlackoutRange("00:00-00:01");

            string actual = this.Preferences.WwanBlackoutRange;

            Assert.Equal("00:00-00:01", actual);
            Assert.Equal("00:00-00:01", actual);
        }

        [Fact]
        public void WhenResetWwanBlackoutRangeThenGetWwanBlackoutRangeShouldReturnNull()
        {
            string range = "00:00-00:01";
            this.Preferences.ResetWwanBlackoutRange();

            range = this.Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenGetWwanBlackoutRangeAndNotCorrectlySavedShouldReturnNull()
        {
            this.Preferences.SetWwanBlackoutRange("00:00-00:01");
            DeleteRegistryValue("WwanBlackoutRange");

            string range = this.Preferences.WwanBlackoutRange;

            Assert.Null(range);
        }

        [Fact]
        public void WhenSettingRoamingBlackoutThenGetRoamingBlackoutShouldReturnExpected()
        {
            this.Preferences.SetRoamingBlackout(true);
            bool roamingBlackout = this.Preferences.RoamingBlackout;

            Assert.True(roamingBlackout);

            this.Preferences.SetRoamingBlackout(false);
            roamingBlackout = this.Preferences.RoamingBlackout;

            Assert.False(roamingBlackout);
        }

        [Fact]
        public void WhenResetRoamingBlackoutThenGetRoamingBlackoutShouldReturnTrue()
        {
            this.Preferences.SetRoamingBlackout(false);
            this.Preferences.ResetRoamingBlackout();
            bool roamingBlackout = this.Preferences.RoamingBlackout;

            Assert.True(roamingBlackout);
        }

        [Fact]
        public void WhenGetRoamingBlackoutAndNotCorrectlySavedReturnTrue()
        {
            this.Preferences.SetRoamingBlackout(false);
            DeleteRegistryValue("RoamingBlackout");
            bool roamingBlackout = this.Preferences.RoamingBlackout;

            Assert.True(roamingBlackout);
        }

        [Fact]
        public void WhenGetRoamingBlackoutAndNeverSetRoamingBeforeShouldReturnFalse()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var actual = this.Preferences.RoamingBlackout;

            Assert.True(actual);
        }

        [Fact]
        public void WhenGetOnChargeBlackoutAndNeverSetRoamingBeforeShouldReturnFalse()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var actual = this.Preferences.OnChargeBlackout;

            Assert.False(actual);
        }

        [Fact]
        public void WhenGetOffChargeBlackoutAndNeverSetRoamingBeforeShouldReturnFalse()
        {
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var actual = this.Preferences.OffChargeBlackout;

            Assert.False(actual);
        }

        [Fact]
        public void WhenSettingOnChargeBlackoutThenGetOnChargeBlackoutShouldReturnExpected()
        {
            this.Preferences.SetOnChargeBlackout(true);
            bool chargingBlackout = this.Preferences.OnChargeBlackout;

            Assert.True(chargingBlackout);

            this.Preferences.SetOnChargeBlackout(false);
            chargingBlackout = this.Preferences.OnChargeBlackout;

            Assert.False(chargingBlackout);
        }

        [Fact]
        public void WhenResetOnChargeBlackoutThenGetOnChargeBlackoutShouldReturnFalse()
        {
            this.Preferences.SetOnChargeBlackout(true);
            this.Preferences.ResetOnChargeBlackout();
            bool chargingBlackout = this.Preferences.OnChargeBlackout;

            Assert.False(chargingBlackout);
        }

        [Fact]
        public void WhenGetOnChargeBlackoutAndNotCorrectlySavedReturnFalse()
        {
            this.Preferences.SetOnChargeBlackout(true);
            DeleteRegistryValue("OnChargeBlackout");
            bool chargingBlackout = this.Preferences.OnChargeBlackout;

            Assert.False(chargingBlackout);
        }

        [Fact]
        public void WhenSettingOffChargeBlackoutThenGetOffChargeBlackoutShouldReturnExpected()
        {
            this.Preferences.SetOffChargeBlackout(true);
            bool dischargingBlackout = this.Preferences.OffChargeBlackout;

            Assert.True(dischargingBlackout);

            this.Preferences.SetOffChargeBlackout(false);
            dischargingBlackout = this.Preferences.OffChargeBlackout;

            Assert.False(dischargingBlackout);
        }

        [Fact]
        public void WhenResetOffChargeBlackoutThenGetOffChargeBlackoutShouldReturnFalse()
        {
            this.Preferences.SetOffChargeBlackout(true);
            this.Preferences.ResetOffChargeBlackout();
            bool dischargingBlackout = this.Preferences.OffChargeBlackout;

            Assert.False(dischargingBlackout);
        }

        [Fact]
        public void WhenGetOffChargeBlackoutAndNotCorrectlySavedReturnFalse()
        {
            this.Preferences.SetOffChargeBlackout(true);
            DeleteRegistryValue("OffChargeBlackout");
            bool dischargingBlackout = this.Preferences.OffChargeBlackout;

            Assert.False(dischargingBlackout);
        }

        [Fact]
        public void IsBlackoutRangeValidShouldReturnExpected()
        {
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30"));
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30,03:30-06:00"));
            Assert.True(Preferences.IsBlackoutRangeValid("00:00-02:30,03:30-06:00,07:00-07:01"));

            Assert.False(Preferences.IsBlackoutRangeValid(null));
            Assert.False(Preferences.IsBlackoutRangeValid(string.Empty));
            Assert.False(Preferences.IsBlackoutRangeValid("00:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1:0002:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:10-"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:10-1"));
            Assert.False(Preferences.IsBlackoutRangeValid("00:00-02:30-15:42"));
            Assert.False(Preferences.IsBlackoutRangeValid("13:00 - 21:00"));
            Assert.False(Preferences.IsBlackoutRangeValid("1300-21:00"));
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
