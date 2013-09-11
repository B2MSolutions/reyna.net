namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase, IService
    {
        public ForwardService(IRepository persistentStore, IHttpClient httpClient)
        {
            if (persistentStore == null)
            {
                throw new ArgumentNullException("persistentStore");
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            this.PersistentStore = persistentStore;
            this.HttpClient = httpClient;
            this.PersistentStore.MessageAdded += OnMessageAdded;
        }

        private IRepository PersistentStore { get; set; }

        private IHttpClient HttpClient { get; set; }
        
        public void Dispose()
        {
            this.Stop();

            if (this.PersistentStore != null)
            {
                this.PersistentStore.MessageAdded -= this.OnMessageAdded;
            }
        }

        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.NewMessageAdded.WaitOne();
                IMessage message = null;

                while ((message = this.PersistentStore.Get()) != null)
                {
                    var result = this.HttpClient.Post(message);
                    if (result == Result.TemporaryError)
                    {
                        break;
                    }

                    this.PersistentStore.Remove();
                }

                this.NewMessageAdded.Reset();
            }
        }
    }
}
