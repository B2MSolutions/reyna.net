namespace Reyna.Facts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Reyna.Power;
    using Xunit;

    public class GivenAPowerManager
    {
        [Fact]
        public void WhenCallingIsBatteryChargingAndBatteryIsChargingShouldReturnTrue()
        {
            NativeMethods.ACLineStatus = 1;
            NativeMethods.SystemPowerStatusExResult = 1;
            Assert.True(new PowerManager().IsBatteryCharging());
        }

        [Fact]
        public void WhenCallingIsBatteryChargingAndBatteryIsNotChargingShouldReturnFalse()
        {
            NativeMethods.ACLineStatus = 0;
            NativeMethods.SystemPowerStatusExResult = 1;
            Assert.False(new PowerManager().IsBatteryCharging());
        }

        [Fact]
        public void WhenCallingIsBatteryChargingAndBatteryFailedToGetPowerStatusShouldReturnFalse()
        {
            NativeMethods.SystemPowerStatusExResult = 0;
            Assert.False(new PowerManager().IsBatteryCharging());
        }
    }
}
