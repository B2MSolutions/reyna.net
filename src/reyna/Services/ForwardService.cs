namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase, IForward
    {
        private const string PeriodicBackoutCheckTAG = "ForwardService";

        public ForwardService(IRepository sourceStore, IHttpClient httpClient, INetworkStateService networkStateService, IWaitHandle waitHandle, int temporaryErrorMilliseconds, int sleepMilliseconds, bool batchUpload, IReynaLogger logger)
            : base(sourceStore, waitHandle, true, logger)
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
            this.ContactInformation = new RegistryContactInformation(new Registry(), @"Software\Reyna");
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

        internal IContactInformation ContactInformation { get; set; }
        
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
                var canSend = this.MessageProvider.CanSend
                    && this.PeriodicBackoutCheck.IsTimeElapsed(PeriodicBackoutCheckTAG, this.TemporaryErrorMilliseconds)
                    && Reyna.HttpClient.CanSend() == Result.Ok;

                this.Logger.Debug("ForwardService.CanSend {0}", canSend);

                return canSend;
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
                try
                {
                    IMessage message = null;

                    if (this.CanSend)
                    {
                        while (!this.Terminate && (message = this.MessageProvider.GetNext()) != null)
                        {
                            this.Logger.Debug("ForwardService.ThreadStart message {0}", message.Id);

                            var result = this.HttpClient.Post(message);

                            this.RecordContactInformation(result);

                            if (result == Result.TemporaryError)
                            {
                                this.Logger.Debug("ForwardService.ThreadStart temporary error");
                                this.PeriodicBackoutCheck.Record(ForwardService.PeriodicBackoutCheckTAG);
                                break;
                            }

                            if (result == Result.Blackout || result == Result.NotConnected)
                            {
                                this.Logger.Debug("ForwardService.ThreadStart blacked out/not connected");
                                break;
                            }

                            this.Logger.Debug("ForwardService.ThreadStart delete and sleep");
                            this.MessageProvider.Delete(message);
                            this.Sleep(this.SleepMilliseconds);
                        }

                        this.MessageProvider.Close();
                    }
                }
                catch (Exception exception)
                {
                    this.Logger.Err("ForwardService.ThreadStart. Error {0}", exception.ToString());
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

        private void RecordContactInformation(Result result)
        {
            var contactTime = DateTime.UtcNow;
            this.ContactInformation.LastContactAttempt = contactTime;
            this.ContactInformation.LastContactResult = result;
            if (result == Result.Ok)
            {
                this.ContactInformation.LastSuccessfulContact = contactTime;
            }
        }
    }
}
