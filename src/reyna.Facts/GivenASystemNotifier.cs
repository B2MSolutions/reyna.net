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
        public void WhenCallingNotifyOnNetworkConnectShouldNotThrow()
        {
            Assert.DoesNotThrow(() => this.SystemNotifier.NotifyOnNetworkConnect(string.Empty));
        }

        [Fact]
        public void WhenCallingClearNotificationShouldNotThrow()
        {
            Assert.DoesNotThrow(() => this.SystemNotifier.ClearNotification(string.Empty));
        }
    }
}
