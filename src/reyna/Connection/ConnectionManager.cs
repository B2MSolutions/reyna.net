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
            this.Preferences = new Preferences();
            this.PowerManager = new PowerManager();
            this.ConnectionInfo = new ConnectionInfo();
        }

        internal Preferences Preferences { get; set; }

        internal PowerManager PowerManager { get; set; }

        internal IConnectionInfo ConnectionInfo { get; set; }

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

            if (this.Preferences.OnChargeBlackout && this.PowerManager.IsBatteryCharging())
            {
                return Result.Blackout;
            }

            if (this.Preferences.OffChargeBlackout && !this.PowerManager.IsBatteryCharging())
            {
                return Result.Blackout;
            }

            if (this.Preferences.RoamingBlackout && this.ConnectionInfo.Roaming)
            {
                return Result.Blackout;
            }

            BlackoutTime blackoutTime = new BlackoutTime();

            if (!HttpClient.CanSendNow(blackoutTime, this.Preferences.WlanBlackoutRange) && this.ConnectionInfo.Wifi)
            {
                return Result.Blackout;
            }

            if (!HttpClient.CanSendNow(blackoutTime, this.Preferences.WwanBlackoutRange) && this.ConnectionInfo.Mobile)
            {
                return Result.Blackout;
            }

            return Result.Ok;
        }
    }
}
