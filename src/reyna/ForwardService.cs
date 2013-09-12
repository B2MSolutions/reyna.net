namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase
    {
        public ForwardService(IRepository sourceStore, IHttpClient httpClient, INetworkStateService networkState, IWaitHandle waitHandle) : base(sourceStore, waitHandle, true)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (networkState == null)
            {
                throw new ArgumentNullException("networkState");
            }

            this.HttpClient = httpClient;
            this.NetworkState = networkState;

            this.NetworkState.NetworkConnected += this.OnNetworkConnected;
        }

        private IHttpClient HttpClient { get; set; }

        private INetworkStateService NetworkState { get; set; }
        
        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.WaitHandle.WaitOne();
                IMessage message = null;

                while (!this.Terminate && (message = this.SourceStore.Get()) != null)
                {
                    var result = this.HttpClient.Post(message);
                    if (result == Result.TemporaryError)
                    {
                        this.WaitHandle.Reset();
                        break;
                    }

                    this.SourceStore.Remove();
                }

                this.WaitHandle.Reset();
            }
        }

        private void OnNetworkConnected(object sender, EventArgs e)
        {
            if (this.Terminate)
            {
                return;
            }

            this.SignalWorkToDo();
        }
    }
}
