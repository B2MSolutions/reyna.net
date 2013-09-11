namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class StoreService : IService
    {
        public StoreService(IRepository volatileStore, IRepository persistentStore)
        {
            if (volatileStore == null)
            {
                throw new ArgumentNullException("messageStore");
            }

            if (persistentStore == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.VolatileStore = volatileStore;
            this.PersistentStore = persistentStore;

            this.VolatileStore.Initialise();
            this.PersistentStore.Initialise();

            this.NewMessageAdded = new AutoResetEvent(false);
            this.VolatileStore.MessageAdded += this.OnMessageAdded;
        }

        private IRepository VolatileStore { get; set; }

        private IRepository PersistentStore { get; set; }

        private AutoResetEvent NewMessageAdded { get; set; }

        private Thread WorkingThread { get; set; }

        private bool Terminate { get; set; }

        public void Start()
        {
            this.WaitForWorkingThreadToFinish();

            this.Terminate = false;
            this.WorkingThread = new Thread(this.ThreadStart);
            this.WorkingThread.Start();
        }

        public void Stop()
        {
            this.Terminate = true;
            this.NewMessageAdded.Set();
            this.WaitForWorkingThreadToFinish();
        }

        public void Dispose()
        {
            this.Stop();

            if (this.VolatileStore != null)
            {
                this.VolatileStore.MessageAdded -= this.OnMessageAdded;
            }
        }

        private void WaitForWorkingThreadToFinish()
        {
            if (this.WorkingThread != null)
            {
                this.WorkingThread.Join();
            }
        }

        private void ThreadStart()
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

        private void OnMessageAdded(object sender, EventArgs e)
        {
            if (this.Terminate)
            {
                return;
            }

            this.NewMessageAdded.Set();
        }
    }
}
