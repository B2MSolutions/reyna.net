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
