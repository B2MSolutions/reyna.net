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

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore);
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient);
        }

        internal IRepository VolatileStore { get; set; }

        internal IRepository PersistentStore { get; set; }

        internal IHttpClient HttpClient { get; set; }

        internal IService StoreService { get; set; }
        
        internal IService ForwardService { get; set; }

        public void Start()
        {
            this.StoreService.Start();
            this.ForwardService.Start();
        }

        public void Stop()
        {
            this.StoreService.Stop();
            this.ForwardService.Stop();
        }

        public void Put(IMessage message)
        {
            this.VolatileStore.Add(message);
        }

        public void Dispose()
        {
            if (this.StoreService != null)
            {
                this.StoreService.Dispose();
            }

            if (this.ForwardService != null)
            {
                this.ForwardService.Dispose();
            }
        }
    }
}
