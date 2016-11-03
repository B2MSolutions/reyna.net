namespace Reyna
{
    using System;

    public interface ITimeProvider
    {
        long GetEpochInMilliSeconds(DateTimeKind dateTimeKind);
    }
}