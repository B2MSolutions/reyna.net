namespace Reyna
{
    using System;
    using System.Net;
    using Reyna.Interfaces;

    internal class BatchProvider : IMessageProvider
    {
        private const string PeriodicBackoutCheckTAG = "BatchProvider";

        public BatchProvider(IRepository repository)
        {
            this.Repository = repository;
            this.BatchConfiguration = new BatchConfiguration();
            this.PeriodicBackoutCheck = new RegistryPeriodicBackoutCheck(new Registry(), @"Software\Reyna\PeriodicBackoutCheck");
        }

        public bool CanSend
        {
            get
            {
                long interval = (long)((this.BatchConfiguration.SubmitInterval * 0.9) / 1000);
                if (this.PeriodicBackoutCheck.IsTimeElapsed(PeriodicBackoutCheckTAG, interval))
                {
                    return true;
                }

                return this.Repository.AvailableMessagesCount >= this.BatchConfiguration.BatchMessageCount;
            }
        }

        internal IBatchConfiguration BatchConfiguration { get; set; }

        internal IPeriodicBackoutCheck PeriodicBackoutCheck { get; set; }

        private IRepository Repository { get; set; }

        public IMessage GetNext()
        {
            var message = this.Repository.Get();
            var batch = new Batch();

            WebHeaderCollection headers = null;
            var count = 0;
            long size = 0;
            long maxMessagesCount = this.BatchConfiguration.BatchMessageCount;
            long maxBatchSize = this.BatchConfiguration.BatchMessagesSize;
            while (message != null && count < maxMessagesCount && size < maxBatchSize)
            {
                headers = message.Headers;
                batch.Add(message);
                
                size = this.GetSize(batch);
                count++;
                message = this.Repository.GetNextMessageAfter(message.Id);
            }

            if (batch.Events.Count > 0)
            {
                var lastMessage = batch.Events[batch.Events.Count - 1];
                var batchMessage = new Message(this.GetBatchUploadUrl(lastMessage.Url), batch.ToJson());
                batchMessage.Id = lastMessage.ReynaId;
                for (int index = 0; index < headers.Count; index++)
                {
                    batchMessage.Headers.Add(headers.Keys[index], headers[index]);
                }

                return batchMessage;
            }

            return null;
        }

        public void Delete(IMessage message)
        {
            this.Repository.Delete(message);
        }

        private Uri GetBatchUploadUrl(Uri uri)
        {
            if (this.BatchConfiguration.BatchUrl != null)
            {
                return this.BatchConfiguration.BatchUrl;
            }

            return this.GetUploadUrlFromMessageUrl(uri);
        }

        private Uri GetUploadUrlFromMessageUrl(Uri uri)
        {
            var path = uri.AbsoluteUri;
            string batchPath = null;
            int index = path.LastIndexOf("/");
            if (index != -1)
            {
                batchPath = path.Substring(0, index) + "/batch";
            }
            else
            {
                batchPath = path + "/batch";
            }

            return new Uri(batchPath);
        }

        private long GetSize(Batch batch)
        {
            return System.Text.Encoding.UTF8.GetByteCount(batch.ToJson());
        }
    }
}
