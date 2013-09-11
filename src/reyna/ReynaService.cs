namespace Reyna
{
    using Reyna.Interfaces;

    public class ReynaService : IReyna
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
    }
}
