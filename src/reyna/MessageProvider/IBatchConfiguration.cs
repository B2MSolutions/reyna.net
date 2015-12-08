namespace Reyna
{
    using System;

    internal interface IBatchConfiguration
    {
        int BatchMessageCount { get; }

        long BatchMessagesSize { get; }
        
        long CheckInterval { get; }

        long SubmitInterval { get; }

        Uri BatchUrl { get; }
    }
}
