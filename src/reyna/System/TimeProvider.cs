namespace Reyna
{
    using System;

    public class TimeProvider : ITimeProvider
    {
        public long GetEpochInMilliSeconds(DateTimeKind dateTimeKind)
        {
            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, dateTimeKind);
            return (long)span.TotalMilliseconds;
        }

        public long GetEpochInMilliSeconds(DateTime time, DateTimeKind dateTimeKind)
        {
            TimeSpan span = time - new DateTime(1970, 1, 1, 0, 0, 0, 0, dateTimeKind);
            return (long)span.TotalMilliseconds;
        }

        public DateTime GetDateTimeFromEpoc(long epoc, DateTimeKind dateTimeKind)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, dateTimeKind).AddMilliseconds(epoc);
        }
    }
}