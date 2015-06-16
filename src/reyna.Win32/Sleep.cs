namespace Reyna
{
    using System.Threading;

    public class Sleep
    {
        public static void Wait(int seconds)
        {
            Thread.Sleep(seconds * 1000);
        }
    }
}
