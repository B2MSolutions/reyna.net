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
                if (NativeMethods.GetSystemPowerStatusEx(systemPowerStatus, 1) == 1)
                {
                    return systemPowerStatus;
                }

                return null;
            }
        }

        internal bool IsBatteryCharging()
        {
            if (this.SystemPowerStatus != null)
            {
                if (this.SystemPowerStatus.ACLineStatus.Equals(PowerManager.Online))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
