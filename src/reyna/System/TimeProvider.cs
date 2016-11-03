namespace Reyna
{
    using System;

    public class TimeProvider : ITimeProvider
    {
        public long GetEpochInMilliSeconds(DateTimeKind dateTimeKind)
        {
            TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, dateTimeKind);
            return (long)span.TotalMilliseconds;
        }
    }
}