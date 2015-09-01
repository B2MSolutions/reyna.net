namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Reyna.Power;

    public class NativeMethods
    {
        internal static byte ACLineStatus { get; set; }

        public static int GetSystemPowerStatusEx(SystemPowerStatus systemPowerStatus, int update)
        {
            systemPowerStatus.ACLineStatus = ACLineStatus;
            return 1;
        }
    }
}
