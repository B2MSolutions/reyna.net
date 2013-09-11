namespace Reyna
{
    using Reyna.Interfaces;

    public class ReynaService : IStore
    {
        public ReynaService()
        {
            this.Repository = new SQLiteRepository();
            this.Forward = new Forward(this.Repository, new HttpClient());
        }

        internal IRepository Repository { get; set; }

        internal IForward Forward { get; set; }

        /*
         * private InMemoryQueue StoreQueue { get; set; }
        
                private Store MemoryQueueConsumer { get; set; }
         * 
                public void Start()
                {
                    this.StoreQueue = new InMemoryQueue();
                    this.MemoryQueueConsumer = new Store(this.StoreQueue);
            
                    this.MemoryQueueConsumer.Start();
                }

                public void Stop()
                {
                    this.MemoryQueueConsumer.Stop();
                }
        */

        public void Put(IMessage message)
        {
            //// this.StoreQueue.Add(message);

            if (this.Repository.DoesNotExist)
            {
                this.Repository.Create();
            }

            this.Repository.Enqueue(message);
            
            // TODO
            // need to run in a separate thread
            this.Forward.Send();
        }
    }
}
