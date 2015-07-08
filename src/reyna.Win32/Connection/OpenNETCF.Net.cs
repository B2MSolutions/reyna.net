namespace OpenNETCF 
{
    using System;
    using System.Net;

    namespace Net.NetworkInformation
    {
        using System.Diagnostics.CodeAnalysis;

        internal enum InterfaceOperationalStatus
        {
            NonOperational = 0,
            Unreachable = 1,
            Disconnected = 2,
            Connecting = 3,
            Connected = 4,
            Operational = 5,
        }

        internal interface INetworkInterface
        {
            string Name { get; }

            IPAddress CurrentIpAddress { get; set; }
        }

        internal class NetworkInterface : INetworkInterface
        {
            internal NetworkInterface()
            {
                this.Name = string.Empty;
            }

            public string Name { get; set; }

            public IPAddress CurrentIpAddress { get; set; }

            internal static INetworkInterface[] NetworkInterfaces { get; set; }

            internal static INetworkInterface[] GetAllNetworkInterfaces()
            {
                return NetworkInterface.NetworkInterfaces;
            }
        }
    }
}