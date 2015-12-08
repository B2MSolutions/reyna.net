namespace Reyna
{
    using System;

    internal class BatchConfiguration : IBatchConfiguration
    {
        public int BatchMessageCount
        {
            get
            {
                return 100;
            }
        }

        public long BatchMessagesSize
        {
            get
            {
                return 300 * 1024;
            }
        }

        public long CheckInterval
        {
            get
            {
                return new Preferences().BatchUploadCheckInterval;
            }
        }

        public long SubmitInterval
        {
            get
            {
                return 24 * 60 * 60 * 1000;
            }
        }

        public Uri BatchUrl
        {
            get
            {
                return new Preferences().BatchUploadUrl;
            }
        }
    }
}
