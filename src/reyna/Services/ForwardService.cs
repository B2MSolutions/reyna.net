namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase, IForward
    {
        private const string PeriodicBackoutCheckTAG = "ForwardService";

        public ForwardService(IRepository sourceStore, IHttpClient httpClient, INetworkStateService networkStateService, IWaitHandle waitHandle, int temporaryErrorMilliseconds, int sleepMilliseconds, bool batchUpload)
            : base(sourceStore, waitHandle, true)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (networkStateService != null)
            {
                this.NetworkStateService = networkStateService;
                this.NetworkStateService.NetworkConnected += this.OnNetworkConnected;
            }

            this.HttpClient = httpClient;            
            this.TemporaryErrorMilliseconds = temporaryErrorMilliseconds;
            this.SleepMilliseconds = sleepMilliseconds;
            this.PeriodicBackoutCheck = new RegistryPeriodicBackoutCheck(new Registry(), @"Software\Reyna\PeriodicBackoutCheck");

            if (batchUpload)
            {
                this.MessageProvider = new BatchProvider(sourceStore, this.PeriodicBackoutCheck);
            }
            else
            {
                this.MessageProvider = new MessageProvider(sourceStore);
            }
        }

        internal IMessageProvider MessageProvider { get; set; }

        internal IPeriodicBackoutCheck PeriodicBackoutCheck { get; set; }

        internal int TemporaryErrorMilliseconds { get; set; }

        internal int SleepMilliseconds { get; set; }

        private IHttpClient HttpClient { get; set; }

        private INetworkStateService NetworkStateService { get; set; }

        private bool CanSend
        {
            get
            {
                return this.MessageProvider.CanSend && this.PeriodicBackoutCheck.IsTimeElapsed(PeriodicBackoutCheckTAG, this.TemporaryErrorMilliseconds);
            }
        }

        public void Resume()
        {
            this.SignalWorkToDo();
        }
        
        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.WaitHandle.WaitOne();
                IMessage message = null;

                if (this.CanSend)
                {
                    while (!this.Terminate && (message = this.MessageProvider.GetNext()) != null)
                    {
                        var result = this.HttpClient.Post(message);
                        if (result == Result.TemporaryError)
                        {
                            this.PeriodicBackoutCheck.Record(ForwardService.PeriodicBackoutCheckTAG);
                            break;
                        }

                        if (result == Result.Blackout || result == Result.NotConnected)
                        {
                            break;
                        }

                        this.MessageProvider.Delete(message);
                        this.Sleep(this.SleepMilliseconds);
                    }

                    this.MessageProvider.Close();
                }

                this.WaitHandle.Reset();
            }
        }

        protected override void OnDispose()
        {
            if (this.NetworkStateService != null)
            {
                this.NetworkStateService.NetworkConnected -= this.OnNetworkConnected;
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
            Reyna.Sleep.Wait(millisecondsTimeout / 1000);
        }
    }
}
