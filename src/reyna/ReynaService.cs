namespace Reyna
{
    using Reyna.Interfaces;

    public sealed class ReynaService : IReyna
    {
        public ReynaService()
        {
            this.VolatileStore = new InMemoryQueue();
            this.PersistentStore = new SQLiteRepository();

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore);
        }

        internal IRepository PersistentStore { get; set; }

        internal IRepository VolatileStore { get; set; }

        internal IService StoreService { get; set; }

        public void Start()
        {
            this.StoreService.Start();
        }

        public void Stop()
        {
            this.StoreService.Stop();
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
        }
    }
}
