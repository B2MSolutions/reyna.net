namespace Reyna.Facts
{
    using System;
    using Moq;
    using Xunit;

    public class GivenARegistryContactInformation
    {
        public GivenARegistryContactInformation()
        {
            this.Registry = new Mock<IRegistry>();
            this.ContactInformation = new RegistryContactInformation(this.Registry.Object, "KEY");
        }

        private Mock<IRegistry> Registry { get; set; }

        private IContactInformation ContactInformation { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ContactInformation);
        }

        [Fact]
        public void SetLastContactAttemptShouldUpdateRegistry()
        {
            var testTime = new DateTime(2017, 12, 1).ToUniversalTime();

            long interval = GetEpocInMilliSeconds(testTime);
            long actualInterval = 0;

            this.Registry.Setup(r => r.SetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Callback<Microsoft.Win32.RegistryKey, string, string, long>((a, b, c, d) => actualInterval = d);

            this.ContactInformation.LastContactAttempt = testTime;

            this.Registry.Verify(r => r.SetQWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "LastContactAttempt", interval));
            Assert.Equal(interval, actualInterval);
        }

        [Fact]
        public void GetLastContactAttemptShouldReturnValueFromRegistry()
        {
            var testTime = new DateTime(2017, 12, 1).ToUniversalTime();
            long interval = GetEpocInMilliSeconds(testTime);

            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(interval);
            DateTime lastContactAttempt = this.ContactInformation.LastContactAttempt;

            Assert.Equal(interval, GetEpocInMilliSeconds(lastContactAttempt));
        }

        [Fact]
        public void SetLastSuccessfulContactShouldUpdateRegistry()
        {
            var testTime = new DateTime(2017, 12, 1).ToUniversalTime();

            long interval = GetEpocInMilliSeconds(testTime);
            long actualInterval = 0;

            this.Registry.Setup(r => r.SetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Callback<Microsoft.Win32.RegistryKey, string, string, long>((a, b, c, d) => actualInterval = d);

            this.ContactInformation.LastSuccessfulContact = testTime;

            this.Registry.Verify(r => r.SetQWord(Microsoft.Win32.Registry.LocalMachine, "KEY", "LastSuccessfulContact", interval));
            Assert.Equal(interval, actualInterval);
        }

        [Fact]
        public void GetLastSuccessfulContactShouldReturnValueFromRegistry()
        {
            var testTime = new DateTime(2017, 12, 1).ToUniversalTime();
            long interval = GetEpocInMilliSeconds(testTime);

            this.Registry.Setup(r => r.GetQWord(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), "LastSuccessfulContact", It.IsAny<long>()))
                .Returns(interval);
            DateTime lastContactAttempt = this.ContactInformation.LastSuccessfulContact;

            Assert.Equal(interval, GetEpocInMilliSeconds(lastContactAttempt));
        }
        
        [Fact]
        public void SetLastContactResultShouldUpdateRegistry()
        {
            var testResult = "Ok";
            var actualResultResult = string.Empty;

            this.Registry.Setup(r => r.SetString(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<Microsoft.Win32.RegistryKey, string, string, string>((a, b, c, d) => actualResultResult = d);

            this.ContactInformation.LastContactResult = Reyna.Interfaces.Result.Ok;

            this.Registry.Verify(r => r.SetString(Microsoft.Win32.Registry.LocalMachine, "KEY", "LastContactResult", testResult));
            Assert.Equal(testResult, actualResultResult);
        }

        [Fact]
        public void GetLastContactResultReturnLastResult()
        {
            this.Registry.Setup(r => r.GetString(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Ok");

            Assert.Equal(Reyna.Interfaces.Result.Ok, this.ContactInformation.LastContactResult);
        }

        [Fact]
        public void GetLastContactResultAndRegistryIsEmptyShouldReturnNotConnected()
        {
            this.Registry.Setup(r => r.GetString(It.IsAny<Microsoft.Win32.RegistryKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            Assert.Equal(Reyna.Interfaces.Result.NotConnected, this.ContactInformation.LastContactResult);
        }
        
        private static long GetEpocInMilliSeconds(DateTime time)
        {
            TimeSpan span = time - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            return (long)span.TotalMilliseconds;
        }
    }
}
