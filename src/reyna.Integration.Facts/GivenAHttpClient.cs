using Xunit.Sdk;

namespace Reyna.Integration.Facts
{
    using System;
    using System.Net;
    using OpenNETCF.Net.NetworkInformation;
    using Reyna.Interfaces;
    using Reyna.Power;
    using Xunit;

    public class GivenAHttpClient
    {
        public GivenAHttpClient()
        {
            NetworkInterface.NetworkInterfaces = new INetworkInterface[0];
            this.Preferences = new Preferences();
            this.Preferences.ResetCellularDataBlackout();
            this.Preferences.ResetWlanBlackoutRange();
            this.Preferences.ResetWwanBlackoutRange();
            this.Preferences.ResetRoamingBlackout();
            this.Preferences.ResetOnChargeBlackout();
            this.Preferences.ResetOffChargeBlackout();
        }

        public Preferences Preferences { get; set; }

        [Fact]
        public void WhenCallingPostWithVaidUrlShouldSucceed()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(null);
            var message = new Message(new Uri("http://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);
            
            Assert.Equal(Result.Ok, result);
        }

        [Fact]
        public void WhenCallingPostWithVaidUrlShouldSucceedUsingHttpsAndAcceptAllPolicy()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(new AcceptAllCertificatePolicy());
            var message = new Message(new Uri("https://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.Ok, result);
        }

        [Fact]
        public void WhenCallingPostWithInvalidUrlShouldReturnPermanentError()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(null);
            var message = new Message(new Uri("http://httpbin.org/post2"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.PermanentError, result);
        }

        [Fact]
        public void WhenCallingPostWithInvalidURLSchemeShouldReturnPermanentError()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(null);
            var message = new Message(new Uri("test://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.PermanentError, result);
        }

        [Fact]
        public void WhenCallingGetStatusCodeWithNullResponseShouldReturnServiceUnavailable()
        {
            var actual = HttpClient.GetStatusCode(null);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, actual);
        }

        [Fact]
        public void WhenCallingCanSendShouldReturnTrue()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            Assert.True(HttpClient.CanSend() == Result.Ok);
        }

        [Fact]
        public void WhenCallingCanSendAndNoNeworkShouldReturnFalse()
        {
            Assert.True(HttpClient.CanSend() == Result.NotConnected);
        }

        [Fact]
        public void WhenCallingPostAndConnectionIsGPRSWithVaidUrlShouldSucceedUsingHttpsAndAcceptAllPolicy()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(new AcceptAllCertificatePolicy());
            var message = new Message(new Uri("https://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.Ok, result);
        }

        [Fact]
        public void WhenCallingPostAndConnectionIsGPRSAndBlackoutIsOutOfRangeShouldReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            var time = DateTime.UtcNow.AddHours(-1);
            var from = new Time(time.Hour, time.Minute);
            var to = new Time(from.MinuteOfDay + 1);
            this.Preferences.SetCellularDataBlackout(new TimeRange(from, to));

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var httpClient = new HttpClient(new AcceptAllCertificatePolicy());
            var message = new Message(new Uri("https://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.Ok, result);
        }

        //[Fact]
        //public void WhenCallingPostAndConnectionIsNotGPRSAndBlackoutShouldReturnBlackout()
        //{
        //    var networkInterface = new NetworkInterface();
        //    networkInterface.CurrentIpAddress = new IPAddress(42);
        //    networkInterface.Name = "wifi";
        //    var from = new Time();
        //    var to = new Time(from.MinuteOfDay + 1);
        //    this.Preferences.SetCellularDataBlackout(new TimeRange(from, to));
        //    NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };
        //    var httpClient = new HttpClient(new AcceptAllCertificatePolicy());
        //    var message = new Message(new Uri("https://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
        //    message.Headers.Add("content-type", "application/json");
        //    message.Headers.Add("param1", "Value1");
        //    message.Headers.Add("param2", "Value2");
        //    message.Headers.Add("param3", "Value3");
        //    var result = httpClient.Post(message);
        //    Assert.Equal(Result.Ok, result);
        //}

        [Fact]
        public void WhenCallingPostAndConnectionNotMobile()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = null;
            networkInterface.Name = "cellular line";
            var from = new Time();
            var to = new Time(from.MinuteOfDay + 1);
            this.Preferences.SetCellularDataBlackout(new TimeRange(from, to));

            ConnectionInfo connectionInfo = new ConnectionInfo();

            Assert.False(connectionInfo.Mobile);
        }

        [Fact]
        public void WhenCallingCanSendAndInsideWlanBlackoutReturnBlackout()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };
            this.Preferences.SetWlanBlackoutRange("00:00-23:59");

            Assert.True(HttpClient.CanSend() == Result.Blackout);
        }

        [Fact]
        public void WhenCallingCanSendAndOutsideWlanBlackoutReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };
            this.Preferences.SetWlanBlackoutRange("02:00-02:01");

            Assert.True(HttpClient.CanSend() == Result.Ok);
        }

        [Fact]
        public void WhenCallingCanSendAndInsideWwanBlackoutReturnBlackout()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };
            this.Preferences.SetWwanBlackoutRange("00:00-23:59");

            Assert.True(HttpClient.CanSend() == Result.Blackout);
        }

        [Fact]
        public void WhenCallingCanSendAndOutsideWwanBlackoutReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };
            this.Preferences.SetWwanBlackoutRange("02:00-02:01");

            Assert.True(HttpClient.CanSend() == Result.Ok);
        }

        [Fact]
        public void WhenCallingCanSendAndRoamingBlackoutReturnBlackout()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            this.Preferences.SetRoamingBlackout(true);
            var registry = new Registry();
            registry.SetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0x200 | 0x32);

            Assert.True(HttpClient.CanSend() == Result.Blackout);
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree("System\\State\\Phone");
        }

        [Fact]
        public void WhenCallingCanSendAndNotRoamingBlackoutReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            this.Preferences.SetRoamingBlackout(false);
            var registry = new Registry();
            registry.SetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0x200 | 0x32);

            Assert.True(HttpClient.CanSend() == Result.Ok);
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree("System\\State\\Phone");
        }

        [Fact]
        public void WhenCallingCanSendAndOnChargeBlackoutAndIsChargingReturnBlackout()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            NativeMethods.ACLineStatus = 1;
            NativeMethods.SystemPowerStatusExResult = 1;
            this.Preferences.SetOnChargeBlackout(true);

            Assert.True(HttpClient.CanSend() == Result.Blackout);
        }

        [Fact]
        public void WhenCallingCanSendAndOnChargeBlackoutAndNotChargingReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            NativeMethods.ACLineStatus = 0;
            NativeMethods.SystemPowerStatusExResult = 1;
            this.Preferences.SetOnChargeBlackout(true);

            Assert.True(HttpClient.CanSend() == Result.Ok);
        }

        [Fact]
        public void WhenCallingCanSendAndOffChargeBlackoutAndNotChargingReturnBlackout()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            NativeMethods.ACLineStatus = 0;
            NativeMethods.SystemPowerStatusExResult = 1;
            this.Preferences.SetOffChargeBlackout(true);

            Assert.True(HttpClient.CanSend() == Result.Blackout);
        }

        [Fact]
        public void WhenCallingCanSendAndOffChargeBlackoutAndIsChargingReturnOk()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            NativeMethods.ACLineStatus = 1;
            NativeMethods.SystemPowerStatusExResult = 1;
            this.Preferences.SetOffChargeBlackout(true);

            Assert.True(HttpClient.CanSend() == Result.Ok);
        }
    }
}
