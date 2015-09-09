namespace Reyna
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using OpenNETCF.Net.NetworkInformation;

    public class ConnectionInfo : IConnectionInfo
    {
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
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (LANNetwork(ni) || ActiveSyncNetwork(ni) || GPRSNetwork(ni))
                    {
                        continue;
                    }

                    return true;
                }

                return false;
            }
        }

        public bool Roaming
        {
            get
            {
                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    bool roamingConnected = false;
                    bool wifiConnected = false;
                    foreach (var ni in interfaces)
                    {
                        if (RoamingNetwork(ni))
                        {
                            if (NetworkConnected(ni))
                            {
                                roamingConnected = true;
                            }
                        }
                        else if (WifiNetwork(ni))
                        {
                            if (NetworkConnected(ni))
                            {
                                wifiConnected = true;
                            }
                        }
                    }

                    return roamingConnected && !wifiConnected;
                }
                catch (Exception)
                {
                }

                return false;
            }
        }

        private static bool RoamingNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("roaming", StringComparison.CurrentCulture);
        }

        private static bool WifiNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("wifi", StringComparison.CurrentCulture);
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
