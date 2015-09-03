namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Reyna.Interfaces;
    using Reyna.Power;

    public class ConnectionManager
    {
        public ConnectionManager()
        {
            this.ConnectionInfo = new ConnectionInfo();
            this.Preferences = new Preferences();
            this.PowerManager = new PowerManager();
        }

        public Preferences Preferences { get; set; }

        public PowerManager PowerManager { get; set; }

        public IConnectionInfo ConnectionInfo { get; set; }

        public Result CanSend(IConnectionInfo info)
        {
            this.ConnectionInfo = info;
            return this.CanSend();
        }

        public Result CanSend()
        {
            if (!this.ConnectionInfo.Connected)
            {
                return Result.NotConnected;
            }

            if (Preferences.WwanBlackoutRange == null)
            {
                HttpClient.SaveCellularDataAsWwanForBackwardsCompatibility();
            }

            if (this.PowerManager.IsBatteryCharging() && this.Preferences.OnChargeBlackout)
            {
                return Result.Blackout;
            }

            if (!this.PowerManager.IsBatteryCharging() && this.Preferences.OffChargeBlackout)
            {
                return Result.Blackout;
            }

            if (this.ConnectionInfo.Roaming && this.Preferences.RoamingBlackout)
            {
                return Result.Blackout;
            }

            BlackoutTime blackoutTime = new BlackoutTime();

            if (this.ConnectionInfo.Wifi && !HttpClient.CanSendNow(blackoutTime, this.Preferences.WlanBlackoutRange))
            {
                return Result.Blackout;
            }

            if (this.ConnectionInfo.Mobile && !HttpClient.CanSendNow(blackoutTime, this.Preferences.WwanBlackoutRange))
            {
                return Result.Blackout;
            }

            return Result.Ok;
        }
    }
}
