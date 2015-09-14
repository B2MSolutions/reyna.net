namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase
    {
        public ForwardService(IRepository sourceStore, IHttpClient httpClient, INetworkStateService networkState, IWaitHandle waitHandle, int temporaryErrorMilliseconds, int sleepMilliseconds)
            : base(sourceStore, waitHandle, true)
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

            this.TemporaryErrorMilliseconds = temporaryErrorMilliseconds;
            this.SleepMilliseconds = sleepMilliseconds;

            this.NetworkState.NetworkConnected += this.OnNetworkConnected;
        }

        internal int TemporaryErrorMilliseconds { get; set; }

        internal int SleepMilliseconds { get; set; }

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
                        this.Sleep(this.TemporaryErrorMilliseconds);
                        this.WaitHandle.Reset();
                        break;
                    }

                    if (result == Result.Blackout || result == Result.NotConnected)
                    {
                        return;
                    }

                    this.SourceStore.Remove();
                    this.Sleep(this.SleepMilliseconds);
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

        private void Sleep(int millisecondsTimeout)
        {
            int timeoutInFiveSecondsPeriod = millisecondsTimeout / (1000 * 5);
            if (timeoutInFiveSecondsPeriod > 1)
            {
                while (!this.Terminate && timeoutInFiveSecondsPeriod > 0)
                {
                    Reyna.Sleep.Wait(5);
                    timeoutInFiveSecondsPeriod--;
                }
            }
            else
            {
                Reyna.Sleep.Wait(millisecondsTimeout / 1000);
            }
        }
    }
}
