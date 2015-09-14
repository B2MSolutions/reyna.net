namespace Reyna
{
    using System.Runtime.InteropServices; 
    using Reyna.Power;

    internal class NativeMethods
    {       
        [DllImport("coredll", SetLastError = true)]
        public static extern int GetSystemPowerStatusEx(SystemPowerStatus systemPowerStatus, int update);
    }
}
