namespace Reyna
{
    using System;

    internal class RegistryPeriodicBackoutCheck : IPeriodicBackoutCheck
    {
        private readonly ITimeProvider timeProvider;

        public RegistryPeriodicBackoutCheck(IRegistry registry, string key)
        {
            this.Registry = registry;
            this.PeriodicalTasksKeyName = key;
            this.timeProvider = new TimeProvider();
        }

        private IRegistry Registry { get; set; }

        private string PeriodicalTasksKeyName { get; set; }

        public void Record(string task)
        {
            long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds();
            this.Registry.SetQWord(Microsoft.Win32.Registry.LocalMachine, this.PeriodicalTasksKeyName, task, epocInMilliseconds);
        }

        public bool IsTimeElapsed(string task, long periodInMilliseconds)
        {
            long lastCheckedTime = this.Registry.GetQWord(Microsoft.Win32.Registry.LocalMachine, this.PeriodicalTasksKeyName, task, 0);
            long epocInMilliseconds = this.timeProvider.GetEpochInMilliSeconds();

            long elapsedPeriodInSeconds = epocInMilliseconds - lastCheckedTime;

            if (lastCheckedTime > epocInMilliseconds)
            {
                this.Record(task);
                return true;
            }

            return elapsedPeriodInSeconds >= periodInMilliseconds;
        }
    }
}
