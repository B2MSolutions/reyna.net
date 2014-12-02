namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;
    
    internal sealed class NetworkStateService : ThreadWorker, INetworkStateService
    {
        public static readonly string NetworkConnectedNamedEvent = "Reyna\\NetworkConnected";

        public NetworkStateService(ISystemNotifier systemNotifier, IWaitHandle waitHandle) : base(waitHandle, false)
        {
            if (systemNotifier == null)
            {
                throw new ArgumentNullException("systemNotifier");
            }
            
            this.SystemNotifier = systemNotifier;
        }
        
        public event EventHandler<EventArgs> NetworkConnected;

        private ISystemNotifier SystemNotifier { get; set; }

        public override void Start()
        {
            base.Start();
            this.SubscribeToNetworkStateChange();
        }

        public override void Stop()
        {
            base.Stop();
            this.UnSubscribeToNetworkStateChange();
        }

        internal void SendNetworkConnectedEvent()
        {
            if (this.Terminate)
            {
                return;
            }

            if (this.NetworkConnected == null)
            {
                return;
            }

            this.NetworkConnected.Invoke(this, EventArgs.Empty);
        }

        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.WaitHandle.WaitOne();

                this.SendNetworkConnectedEvent();

                this.WaitHandle.Reset();
            }
        }

        private void SubscribeToNetworkStateChange()
        {
            this.SystemNotifier.NotifyOnNetworkConnect(NetworkStateService.NetworkConnectedNamedEvent);
        }

        private void UnSubscribeToNetworkStateChange()
        {
            this.SystemNotifier.ClearNotification(NetworkStateService.NetworkConnectedNamedEvent);
        }
    }
}
