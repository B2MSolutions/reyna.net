namespace Reyna.Facts
{
    using Moq;
    using OpenNETCF.Net.NetworkInformation;
    using Xunit;
    using Xunit.Extensions;

    public class GivenAConnectionInfo
    {
        public GivenAConnectionInfo()
        {
            this.ConnectionInfo = new ConnectionInfo();
            this.ConnectionInfoMock = new Mock<IConnectionInfo>();
            this.Registry = new Mock<IRegistry>();
            this.ConnectionInfo.Registry = this.Registry.Object;
        }

        private Mock<IConnectionInfo> ConnectionInfoMock { get; set; }

        private Mock<IRegistry> Registry { get; set; }

        private ConnectionInfo ConnectionInfo { get; set; }

        [Fact]
        public void Construction()
        {
            this.ConnectionInfo = new ConnectionInfo();
            Assert.NotNull(this.ConnectionInfo);
            Assert.NotNull(this.ConnectionInfo.Registry);
        }

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

        [Fact]
        public void WhenCallingRoamingAndDeviceNoSupportingPhone()
        {
            this.Registry.Setup(r => r.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0))
                .Returns(0);

            Assert.False(this.ConnectionInfo.Roaming);
            this.Registry.Verify(r => r.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0), Times.Once());
        }

        [Fact]
        public void WhenCallingRoamingAndDeviceNotRoamingShouldReturnFalse()
        {
            this.Registry.Setup(r => r.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0))
                .Returns(0x32);

            Assert.False(this.ConnectionInfo.Roaming);

            this.Registry.Verify(r => r.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0), Times.Once());
        }

        [Fact]
        public void WhenCallingRoamingAndDeviceIsRoamingShouldReturnTrue()
        {
            this.Registry.Setup(r => r.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0))
                .Returns(0x32 | 0x200);

            Assert.True(this.ConnectionInfo.Roaming);
        }

        [Fact]
        public void WhenCallingWifiAndWirelessZeroConfigNetworkInterfaceExistsShouldReturnTrue()
        {
            var wirelessInterface = new WirelessZeroConfigNetworkInterface();
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { wirelessInterface };

            Assert.True(this.ConnectionInfo.Wifi);
        }

        [Fact]
        public void WhenCallingWifiAndWirelessNetworkInterfaceExistsShouldReturnTrue()
        {
            var wirelessInterface = new WirelessNetworkInterface();
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { wirelessInterface };

            Assert.True(this.ConnectionInfo.Wifi);
        }

        [Theory]
        [InlineData(10000000)]
        [InlineData(100000000)]
        public void WhenCallingWifiAndLanNetworkInterfaceExistsShouldReturnFalse(int speed)
        {
            var networkInterface = new NetworkInterface();
            networkInterface.Speed = speed;
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Wifi);
        }

        [Theory]
        [InlineData("USB Cable")]
        [InlineData("usb cable")]
        public void WhenCallingWifiAndActiveSyncNetworkInterfaceExistsShouldReturnFalse(string name)
        {
            var networkInterface = new NetworkInterface();
            networkInterface.Name = name;
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Wifi);
        }

        [Theory]
        [InlineData("Cellular Line")]
        [InlineData("cellular line")]
        public void WhenCallingWifiAndGPRSNetworkInterfaceExistsShouldReturnFalse(string name)
        {
            var networkInterface = new NetworkInterface();
            networkInterface.Name = name;
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.False(this.ConnectionInfo.Wifi);
        }

        [Fact]
        public void WhenCallingWifiAndMoreThanOneNetworkInterfaceExistsAndFirstOneIsUSBSkipItAndUseFirstWireless()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.Name = "USB Cable";
            var wirelessInterface = new WirelessNetworkInterface();

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface, wirelessInterface };

            Assert.True(this.ConnectionInfo.Wifi);
        }

        [Fact]
        public void WhenCallingWifiAndMoreThanOneNetworkInterfaceExistsAndNoneIsWirelessNetworkInterface()
        {
            var usbNetworkInterface = new NetworkInterface();
            usbNetworkInterface.Name = "USB Cable";

            var lanNetworkInterface = new NetworkInterface();
            lanNetworkInterface.Speed = 10000000;

            var wirelessInterface = new NetworkInterface();
            wirelessInterface.Name = "WIRELESS";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { usbNetworkInterface, lanNetworkInterface, wirelessInterface };

            Assert.True(this.ConnectionInfo.Wifi);
        }

        [Fact]
        public void WhenCallingWifiAndNotUsingWifiShouldReturnFalse()
        {
            var usbNetworkInterface = new NetworkInterface();
            usbNetworkInterface.Name = "USB Cable";

            var lanNetworkInterface = new NetworkInterface();
            lanNetworkInterface.Speed = 10000000;

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { usbNetworkInterface, lanNetworkInterface };
            Assert.False(this.ConnectionInfo.Wifi);
        }
    }
}
