namespace Reyna
{
    using System;

    public class TimeProvider : ITimeProvider
    {
        public long GetEpochInMilliSeconds()
        {
            TimeSpan span = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            return (long)span.TotalMilliseconds;
        }
    }
}