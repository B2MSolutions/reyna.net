namespace Reyna
{
    using System;

    internal class RegistryPeriodicBackoutCheck : IPeriodicBackoutCheck
    {
        public RegistryPeriodicBackoutCheck(IRegistry registry, string key)
        {
            this.Registry = registry;
            this.PeriodicalTasksKeyName = key;
        }

        private IRegistry Registry { get; set; }

        private string PeriodicalTasksKeyName { get; set; }

        public void Record(string task)
        {
            long epocInMilliseconds = this.GetEpocInMilliSeconds();
            this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.PeriodicalTasksKeyName, task, epocInMilliseconds);
        }

        public bool IsTimeElapsed(string task, long periodInMilliseconds)
        {
            long lastCheckedTime = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.PeriodicalTasksKeyName, task, 0);
            long epocInMilliseconds = this.GetEpocInMilliSeconds();

            long elapsedPeriodInSeconds = epocInMilliseconds - lastCheckedTime;

            if (lastCheckedTime > epocInMilliseconds)
            {
                this.Record(task);
                return true;
            }

            return elapsedPeriodInSeconds >= periodInMilliseconds;
        }

        private long GetEpocInMilliSeconds()
        {
            TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            return (long)span.TotalMilliseconds;
        }
    }
}
