﻿namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class ForwardService : ServiceBase, IForward
    {
        public ForwardService(IRepository sourceStore, IHttpClient httpClient, INetworkStateService networkStateService, IWaitHandle waitHandle, int temporaryErrorMilliseconds, int sleepMilliseconds)
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
        }

        internal int TemporaryErrorMilliseconds { get; set; }

        internal int SleepMilliseconds { get; set; }

        private IHttpClient HttpClient { get; set; }

        private INetworkStateService NetworkStateService { get; set; }

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

                while (!this.Terminate && (message = this.SourceStore.Get()) != null)
                {
                    var result = this.HttpClient.Post(message);
                    if (result == Result.TemporaryError)
                    {
                        this.Sleep(this.TemporaryErrorMilliseconds);
                        break;
                    }

                    if (result == Result.Blackout || result == Result.NotConnected)
                    {
                        break;
                    }

                    this.SourceStore.Remove();
                    this.Sleep(this.SleepMilliseconds);
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
