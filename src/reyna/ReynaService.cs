namespace Reyna
{
    using Reyna.Interfaces;

    public sealed class ReynaService : IReyna
    {
        public ReynaService()
        {
            this.VolatileStore = new InMemoryQueue();
            this.PersistentStore = new SQLiteRepository();
            this.HttpClient = new HttpClient();

            this.StoreWaitHandle = new AutoResetEventAdapter(false);
            this.ForwardWaitHandle = new AutoResetEventAdapter(false);
            this.NetworkWaitHandle = new NamedWaitHandle(false, Reyna.NetworkStateService.NetworkConnectedNamedEvent);

            this.SystemNotifier = new SystemNotifier();

            this.NetworkStateService = new NetworkStateService(this.SystemNotifier, this.NetworkWaitHandle);

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore, this.StoreWaitHandle);
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient, this.NetworkStateService, this.ForwardWaitHandle);
        }

        internal IRepository VolatileStore { get; set; }

        internal IRepository PersistentStore { get; set; }

        internal IHttpClient HttpClient { get; set; }

        internal IService StoreService { get; set; }
        
        internal IService ForwardService { get; set; }

        internal INetworkStateService NetworkStateService { get; set; }

        internal IWaitHandle StoreWaitHandle { get; set; }

        internal IWaitHandle ForwardWaitHandle { get; set; }

        internal IWaitHandle NetworkWaitHandle { get; set; }

        internal ISystemNotifier SystemNotifier { get; set; }

        public void Start()
        {
            this.StoreService.Start();
            this.ForwardService.Start();
            this.NetworkStateService.Start();
        }

        public void Stop()
        {
            this.NetworkStateService.Stop();
            this.ForwardService.Stop();
            this.StoreService.Stop();
        }

        public void Put(IMessage message)
        {
            this.VolatileStore.Add(message);
        }

        public void Dispose()
        {
            if (this.NetworkStateService != null)
            {
                this.NetworkStateService.Dispose();
            }

            if (this.ForwardService != null)
            {
                this.ForwardService.Dispose();
            }

            if (this.StoreService != null)
            {
                this.StoreService.Dispose();
            }
        }
    }
}
