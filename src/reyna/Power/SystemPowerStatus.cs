namespace Reyna.Power
{
    public class SystemPowerStatus
    {
        public int NumLevels { get; set; }

        public int BatteryLifetime { get; set; }

        public int BatteryFullLifetime { get; set; }

        public int BackupBatteryLifetime { get; set; }

        public int BackupBatteryFullLifetime { get; set; }

        public byte ACLineStatus { get; set; }

        public byte BatteryStatus { get; set; }

        public byte BatteryLifePercent { get; set; }

        public byte BackupBattery { get; set; }

        public byte BackupBatteryLifePercent { get; set; }
    }
}
