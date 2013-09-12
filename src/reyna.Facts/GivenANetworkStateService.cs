namespace Reyna.Facts
{
    using System;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenANetworkStateService
    {
        public GivenANetworkStateService()
        {
            this.SystemNotifier = new Mock<ISystemNotifier>();
            this.WaitHandle = new AutoResetEventAdapter(false);

            this.NetworkStateService = new NetworkStateService(this.SystemNotifier.Object, this.WaitHandle);
        }

        private Mock<ISystemNotifier> SystemNotifier { get; set; }

        private IWaitHandle WaitHandle { get; set; }

        private NetworkStateService NetworkStateService { get; set; }

        [Fact]
        public void WhenConstructiingShouldNotThrow()
        {
            Assert.NotNull(this.NetworkStateService);
        }

        [Fact]
        public void WhenConstructingWithAllNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new NetworkStateService(null, null));
            Assert.Equal("waitHandle", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingAndSystemNotifierIsNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new NetworkStateService(null, this.WaitHandle));
            Assert.Equal("systemNotifier", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingAndWaitHandleIsNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new NetworkStateService(this.SystemNotifier.Object, null));
            Assert.Equal("waitHandle", exception.ParamName);
        }

        [Fact]
        public void WhenCallingStartShouldSubscribeToNetworkConnectEvent()
        {
            this.NetworkStateService.Start();

            this.SystemNotifier.Verify(s => s.NotifyOnNetworkConnect("\\\\.\\Notifications\\NamedEvents\\NetworkConnected"), Times.Once());
        }

        [Fact]
        public void WhenCallingStopShouldClearSubscriptionToNetworkConnectEvent()
        {
            this.NetworkStateService.Stop();

            this.SystemNotifier.Verify(s => s.ClearNotification("\\\\.\\Notifications\\NamedEvents\\NetworkConnected"), Times.Once());
        }

        [Fact]
        public void WhenCallingDisposeShouldClearSubscriptionToNetworkConnectEvent()
        {
            this.NetworkStateService.Dispose();

            this.SystemNotifier.Verify(s => s.ClearNotification("\\\\.\\Notifications\\NamedEvents\\NetworkConnected"), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndNetworkConnectedShouldNotifySubscribers()
        {
            var connectedEventFired = false;
            this.NetworkStateService.NetworkConnected += (sender, args) => { connectedEventFired = true; };
            this.WaitHandle.Set();

            this.NetworkStateService.Start();
            System.Threading.Thread.Sleep(100);

            Assert.True(connectedEventFired);
            this.SystemNotifier.Verify(s => s.NotifyOnNetworkConnect("\\\\.\\Notifications\\NamedEvents\\NetworkConnected"), Times.Once());
        }

        [Fact]
        public void WhenCallingStopAndNetworkConnectedFiredShouldNotNotifySubscribers()
        {
            var connectedEventFired = 0;
            this.NetworkStateService.NetworkConnected += (sender, args) => { connectedEventFired++; };
            this.WaitHandle.Set();

            this.NetworkStateService.Start();
            System.Threading.Thread.Sleep(100);

            this.NetworkStateService.Stop();
            this.WaitHandle.Set();

            Assert.Equal(1, connectedEventFired);
        }
    }
}
