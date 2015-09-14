namespace Reyna.Power
{
    public class PowerManager
    {
        public const byte Online = 0x01;

        public SystemPowerStatus SystemPowerStatus
        {
            get
            {
                var systemPowerStatus = new SystemPowerStatus();
                if (NativeMethods.GetSystemPowerStatusEx(systemPowerStatus, 0) == 1)
                {
                    return systemPowerStatus;
                }

                return null;
            }
        }

        internal bool IsBatteryCharging()
        {
            var systemPowerStatus = this.SystemPowerStatus;
            if (systemPowerStatus != null)
            {
                if (systemPowerStatus.ACLineStatus.Equals(PowerManager.Online))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
