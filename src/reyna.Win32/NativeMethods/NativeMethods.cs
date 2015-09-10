namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Reyna.Power;

    public class NativeMethods
    {
        internal static int SystemPowerStatusExResult { get; set; }

        internal static byte ACLineStatus { get; set; }

        public static int GetSystemPowerStatusEx(SystemPowerStatus systemPowerStatus, int update)
        {
            systemPowerStatus.ACLineStatus = ACLineStatus;
            systemPowerStatus.BackupBattery = 0;
            systemPowerStatus.BackupBatteryFullLifetime = 0;
            systemPowerStatus.BackupBatteryLifePercent = 1;
            systemPowerStatus.BackupBatteryLifetime = 1;
            systemPowerStatus.BatteryFullLifetime = 1;
            systemPowerStatus.BatteryLifePercent = 1;
            systemPowerStatus.BatteryLifetime = 1;
            systemPowerStatus.BatteryStatus = 1;
            systemPowerStatus.NumLevels = 1;

            return SystemPowerStatusExResult;
        }
    }
}
