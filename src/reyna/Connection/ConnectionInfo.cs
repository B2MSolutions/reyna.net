namespace Reyna
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using OpenNETCF.Net.NetworkInformation;

    public class ConnectionInfo : IConnectionInfo
    {
        public ConnectionInfo()
        {
            this.Registry = new Registry();
        }
        
        public bool Connected
        {
            get
            {
                try
                {
                    foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (NetworkConnected(ni))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception)
                { 
                }
                
                return false;
            }
        }

        public bool Mobile
        {
            get
            {
                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    bool mobileNetworkConnected = false;
                    bool otherNetworksConnected = false;
                    foreach (var ni in interfaces)
                    {
                        if (GPRSNetwork(ni))
                        {
                            if (NetworkConnected(ni))
                            {
                                mobileNetworkConnected = true;
                            }
                        }
                        else if (NetworkConnected(ni))
                        {
                            otherNetworksConnected = true;
                        }
                    }

                    return mobileNetworkConnected && !otherNetworksConnected;
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        public bool Wifi
        {
            get
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                var wireless = ConnectionInfo.FindWireless(networkInterfaces);
                if (wireless != null)
                {
                    return true;
                }

                var networkInterface = FindWirelessByExclusion(networkInterfaces);
                if (networkInterface != null)
                {
                    return true;
                }

                return false;
            }
        }

        public bool Roaming
        {
            get
            {
                var phoneRoamingBitMask = 0x200;
                var status = this.Registry.GetDWord(Microsoft.Win32.Registry.LocalMachine, "System\\State\\Phone", "Status", 0);
                return (status & phoneRoamingBitMask) == phoneRoamingBitMask;
            }
        }

        internal IRegistry Registry { get; set; }

        private static WirelessNetworkInterface FindWireless(INetworkInterface[] interfaces)
        {
            foreach (var ni in interfaces)
            {
                var wni = ni as WirelessNetworkInterface;
                if (wni != null)
                {
                    return wni;
                }
            }

            return null;
        }

        private static INetworkInterface FindWirelessByExclusion(INetworkInterface[] interfaces)
        {
            foreach (var ni in interfaces)
            {
                if (LANNetwork(ni) || ActiveSyncNetwork(ni) || GPRSNetwork(ni))
                {
                    continue;
                }

                return ni;
            }

            return null;
        }

        private static bool GPRSNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("cellular line", StringComparison.CurrentCulture);
        }

        private static bool ActiveSyncNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("usb", StringComparison.CurrentCulture);
        }

        private static bool LANNetwork(INetworkInterface ni)
        {
            return ni.Speed == 10000000 || ni.Speed == 100000000;
        }

        private static bool NetworkConnected(INetworkInterface networkInterface)
        {
            return string.Compare(networkInterface.CurrentIpAddress.ToString().Trim(), "0.0.0.0", StringComparison.OrdinalIgnoreCase) != 0;
        }
    }
}
