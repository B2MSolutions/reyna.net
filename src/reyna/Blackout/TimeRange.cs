namespace Reyna
{
    public class TimeRange
    {
        public TimeRange(Time from, Time to)
        {
            this.From = from;
            this.To = to;
        }

        public Time From { get; private set; }

        public Time To { get; private set; }

        public bool Contains(Time time)
        {
            if (this.From.MinuteOfDay == this.To.MinuteOfDay)
            {
                return false;
            }

            if (this.To.CompareTo(this.From) >= 0)
            {
               return time.CompareTo(this.From) >= 0 && time.CompareTo(this.To) <= 0;
            }

            return time.CompareTo(this.From) >= 0 || time.CompareTo(this.To) <= 0;
        }
    }
}
