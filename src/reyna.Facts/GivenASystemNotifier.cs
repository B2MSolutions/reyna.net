namespace Reyna.Facts
{
    using System;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenASystemNotifier
    {
        public GivenASystemNotifier()
        {
            this.SystemNotifier = new SystemNotifier();
        }

        private ISystemNotifier SystemNotifier { get; set; }

        [Fact]
        public void WhenCallingNotifyOnNetworkConnectShouldThrow()
        {
            Assert.Throws<NotImplementedException>(() => this.SystemNotifier.NotifyOnNetworkConnect(string.Empty));
        }

        [Fact]
        public void WhenCallingClearNotificationShouldThrow()
        {
            Assert.Throws<NotImplementedException>(() => this.SystemNotifier.ClearNotification(string.Empty));
        }
    }
}
