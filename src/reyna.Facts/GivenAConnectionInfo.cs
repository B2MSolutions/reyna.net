namespace Reyna.Facts
{
    using Moq;
    using OpenNETCF.Net.NetworkInformation;
    using Xunit;

    public class GivenAConnectionInfo
    {
        public GivenAConnectionInfo()
        {
            this.ConnectionInfo = new ConnectionInfo();
        }

        private ConnectionInfo ConnectionInfo { get; set; }

        [Fact]
        public void WhenNetworkInterfaceWithIpExistsShouldReturnTrueForHasConnection()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.True(this.ConnectionInfo.Connected);
        }

        [Fact]
        public void WhenNetworkInterfaceWithIpExistsShouldReturnFalseForHasConnection()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(0);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Connected);
        }

        [Fact]
        public void WhenCallingMobileAndOnlyWifiAvailableShouldReturnFalse()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingConnectedAndExceptionShouldReturnFalse()
        {
            var networkInterface = new NetworkInterface();
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Connected);
        }

        [Fact]
        public void WhenCallingMobileAndOnlyGPRSAvailableShouldReturnTrue()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(42);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.True(this.ConnectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingMobileAndOnlyGPRSAvailableButNotConnectedShouldReturnFalse()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(0);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingMobileAndGPRSAndWifiConnectedShouldReturnFalse()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(43);
            networkInterface.Name = "cellular line";

            var wifi = new NetworkInterface();
            wifi.CurrentIpAddress = new System.Net.IPAddress(43);
            wifi.Name = "wifi";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface, wifi };

            Assert.False(this.ConnectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingMobileAndGPRSIsConnectedAndWifiNotConnectedShouldReturnTrue()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new System.Net.IPAddress(43);
            networkInterface.Name = "cellular line";

            var wifi = new NetworkInterface();
            wifi.CurrentIpAddress = new System.Net.IPAddress(0);
            wifi.Name = "wifi";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface, wifi };

            Assert.True(this.ConnectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingMobileAndThrowsShouldReturnFalse()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.Name = "cellular line";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Mobile);
        }
    }
}
