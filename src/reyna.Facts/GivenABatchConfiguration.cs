namespace Reyna.Facts
{
    using System;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenABatchConfiguration
    {
        public GivenABatchConfiguration()
        {
            this.BatchConfiguration = new BatchConfiguration();
        }

        private BatchConfiguration BatchConfiguration { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.BatchConfiguration);
        }

        [Fact]
        public void BatchMessagesSizeShouldBe300K()
        {
            Assert.Equal(300 * 1024, this.BatchConfiguration.BatchMessagesSize);
        }

        [Fact]
        public void BatchMessageCountShouldBe100()
        {
            Assert.Equal(100, this.BatchConfiguration.BatchMessageCount);
        }

        [Fact]
        public void SubmitIntervalShouldBeOneDay()
        {
            Assert.Equal(24 * 60 * 60 * 1000, this.BatchConfiguration.SubmitInterval);
        }

        [Fact]
        public void BatchUploadUrlShouldReturnFronPreferences()
        {
            new Preferences().SaveBatchUploadUrl(new Uri("http://post2.net"));
            Assert.Equal("http://post2.net/", this.BatchConfiguration.BatchUrl.ToString());
        }

        [Fact]
        public void CheckIntervalShouldReturnFronPreferences()
        {
            new Preferences().SaveBatchUploadCheckInterval(100);
            Assert.Equal(100, this.BatchConfiguration.CheckInterval);
        }
    }
}
