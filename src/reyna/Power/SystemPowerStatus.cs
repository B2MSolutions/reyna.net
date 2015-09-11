namespace Reyna.Power
{
    public class SystemPowerStatus
    {
        public byte ACLineStatus { get; set; }

        public byte BatteryStatus { get; set; }

        public byte BatteryLifePercent { get; set; }

        public byte Reserved1 { get; set; }

        public uint BatteryLifetime { get; set; }

        public uint BatteryFullLifetime { get; set; }

        public byte Reserved2 { get; set; }

        public byte BackupBattery { get; set; }

        public byte BackupBatteryLifePercent { get; set; }

        public byte Reserved3 { get; set; }

        public uint BackupBatteryLifetime { get; set; }

        public uint BackupBatteryFullLifetime { get; set; }
    }
}
