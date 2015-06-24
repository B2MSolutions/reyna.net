namespace Reyna
{
    using System;

    public class Time : IComparable<Time>
    {
        public Time(int hour, int minute)
        {
            if (hour >= 24 || minute >= 60 || hour < 0 || minute < 0)
            {
                throw new ArgumentException("Invalid time");
            }

            this.MinuteOfDay = (hour * 60) + minute;
        }

        public Time(int minuteOfDay)
        {
            if (minuteOfDay < 0 || minuteOfDay >= 1440)
            {
                throw new ArgumentException("Invalid minute of day");
            }

            this.MinuteOfDay = minuteOfDay;
        }

        public Time()
        {
            this.MinuteOfDay = (DateTime.Now.Hour * 60) + DateTime.Now.Minute;
        }

        public int MinuteOfDay { get; private set; }

        public int CompareTo(Time other)
        {
            if (this.MinuteOfDay == other.MinuteOfDay)
            {
                return 0;
            }

            return this.MinuteOfDay < other.MinuteOfDay ? -1 : 1;
        }
    }
}
