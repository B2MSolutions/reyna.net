namespace Reyna.Facts
{
    using System;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenARegistryPeriodicBackoutCheck
    {
        public GivenARegistryPeriodicBackoutCheck()
        {
            this.Registry = new Mock<IRegistry>();
            this.PeriodicBackoutCheck = new RegistryPeriodicBackoutCheck(this.Registry.Object, "KEY");
        }

        private Mock<IRegistry> Registry { get; set; }

        private RegistryPeriodicBackoutCheck PeriodicBackoutCheck { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.PeriodicBackoutCheck);
        }

        [Fact]
        public void RecordShouldUpdateLastTime()
        {
            long interval = 0;
            this.Registry.Setup(r => r.SetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Callback<Microsoft.Win32.RegistryKey, string, string, long>((a, b, c, d) => interval = d);

            this.PeriodicBackoutCheck.Record("task");

            this.Registry.Verify(r => r.SetQWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "task", interval));
            Assert.True(interval > GetEpocInMilliSeconds(DateTime.Now.AddSeconds(-2)) && interval <= GetEpocInMilliSeconds(DateTime.Now));
        }

        [Fact]
        public void TimeElapsedForTaskLastRunBeforeIntervalShouldReturnFalse()
        {
            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(GetEpocInMilliSeconds(DateTime.Now.AddMinutes(-16)));

            var actual = this.PeriodicBackoutCheck.IsTimeElapsed("task", 60 * 60 * 1000);

            Assert.False(actual);
        }

        [Fact]
        public void TimeElapsedForTaskLastRunAfterIntervalShouldReturnTrue()
        {
            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(GetEpocInMilliSeconds(DateTime.Now.AddHours(-1).AddMinutes(-16)));

            var actual = this.PeriodicBackoutCheck.IsTimeElapsed("task", 60 * 60);

            Assert.True(actual);
        }

        [Fact]
        public void TimeElapsedForLastRunInFutureShouldReturnTrue()
        {
            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
            .Returns(GetEpocInMilliSeconds(DateTime.Now.AddHours(1)));

            long interval = 0;
            this.Registry.Setup(r => r.SetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Callback<Microsoft.Win32.RegistryKey, string, string, long>((a, b, c, d) => interval = d);

            var actual = this.PeriodicBackoutCheck.IsTimeElapsed("task", 60 * 60);

            Assert.True(actual);
            this.Registry.Verify(r => r.SetQWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "task", interval));
            Assert.True(interval > GetEpocInMilliSeconds(DateTime.Now.AddSeconds(-2)) && interval <= GetEpocInMilliSeconds(DateTime.Now));
        }

        [Fact]
        public void TimeElapsedForTaskForFirstTimeShouldReturnTrue()
        {
            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                   .Returns(-1);

            var actual = this.PeriodicBackoutCheck.IsTimeElapsed("task", 60 * 60);

            Assert.True(actual);
        }

        [Fact]
        public void TimeElapsedForTaskForFirstTimeShouldReturnTrueWhenReturn0()
        {
            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                        .Returns(0);

            var actual = this.PeriodicBackoutCheck.IsTimeElapsed("task", 60 * 60);

            Assert.True(actual);
        }

        private static long GetEpocInMilliSeconds(DateTime time)
        {
            TimeSpan span = time - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            return (long)span.TotalMilliseconds;
        }
    }
}
