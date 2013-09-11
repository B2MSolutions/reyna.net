namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class StoreService : ServiceBase, IService
    {
        public StoreService(IRepository volatileStore, IRepository persistentStore) : base()
        {
            if (volatileStore == null)
            {
                throw new ArgumentNullException("volatileStore");
            }

            if (persistentStore == null)
            {
                throw new ArgumentNullException("persistentStore");
            }

            this.VolatileStore = volatileStore;
            this.PersistentStore = persistentStore;

            this.VolatileStore.Initialise();
            this.PersistentStore.Initialise();

            this.VolatileStore.MessageAdded += this.OnMessageAdded;
        }

        private IRepository VolatileStore { get; set; }

        private IRepository PersistentStore { get; set; }

        public void Dispose()
        {
            this.Stop();

            if (this.VolatileStore != null)
            {
                this.VolatileStore.MessageAdded -= this.OnMessageAdded;
            }
        }

        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.NewMessageAdded.WaitOne();
                IMessage message = null;

                while ((message = this.VolatileStore.Get()) != null)
                {
                    this.PersistentStore.Add(message);

                    this.VolatileStore.Remove();
                }

                this.NewMessageAdded.Reset();
            }
        }
    }
}
