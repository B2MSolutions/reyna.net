namespace Reyna
{
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Sleep
    {
        public static void Wait(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                Thread.Sleep(1 * 1000);
                NativeMethods.SystemIdleTimerReset();
            }
        }

        private static class NativeMethods
        {
            [DllImport("coredll.dll", SetLastError = true)]
            internal static extern void SystemIdleTimerReset();
        }
    }
}
