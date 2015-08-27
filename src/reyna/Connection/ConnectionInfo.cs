namespace Reyna
{
    using System;
    using System.IO;
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
                try
                {
                    var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var ni in interfaces)
                    {
                        if (WifiNetwork(ni))
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

        private static bool WifiNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("wifi", StringComparison.CurrentCulture);
        }

        private static bool GPRSNetwork(INetworkInterface ni)
        {
            return ni.Name.ToLower(System.Globalization.CultureInfo.CurrentCulture).StartsWith("cellular line", StringComparison.CurrentCulture);
        }

        private static bool NetworkConnected(INetworkInterface networkInterface)
        {
            return string.Compare(networkInterface.CurrentIpAddress.ToString().Trim(), "0.0.0.0", StringComparison.OrdinalIgnoreCase) != 0;
        }
    }
}
