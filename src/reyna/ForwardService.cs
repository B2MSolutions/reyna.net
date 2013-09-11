namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase
    {
        public ForwardService(IRepository sourceStore, IHttpClient httpClient) : base(sourceStore)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            this.HttpClient = httpClient;
        }

        private IHttpClient HttpClient { get; set; }
        
        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.DoWorkEvent.WaitOne();
                IMessage message = null;

                while (!this.Terminate && (message = this.SourceStore.Get()) != null)
                {
                    var result = this.HttpClient.Post(message);
                    if (result == Result.TemporaryError)
                    {
                        break;
                    }

                    this.SourceStore.Remove();
                }

                this.DoWorkEvent.Reset();
            }
        }
    }
}
