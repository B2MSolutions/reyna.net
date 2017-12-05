namespace Reyna
{
    using System;

    public interface ITimeProvider
    {
        long GetEpochInMilliSeconds(DateTimeKind dateTimeKind);

        long GetEpochInMilliSeconds(DateTime time, DateTimeKind dateTimeKind);

        DateTime GetDateTimeFromEpoc(long epoc, DateTimeKind dateTimeKind);
    }
}