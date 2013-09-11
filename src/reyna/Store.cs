namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    public class Store : IDisposable
    {
        public Store(IMessageStore messageStore, IRepository repository)
        {
            if (messageStore == null)
            {
                throw new ArgumentNullException("messageStore");
            }

            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            this.MessageStore = messageStore;
            this.Repository = repository;
            this.NewMessageAdded = new AutoResetEvent(false);
            this.MessageStore.MessageAdded += this.OnMessageAdded;
        }

        private IMessageStore MessageStore { get; set; }

        private IRepository Repository { get; set; }

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

            if (this.MessageStore != null)
            {
                this.MessageStore.MessageAdded -= this.OnMessageAdded;
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

                while ((message = this.MessageStore.Get()) != null)
                {
                    this.Repository.Enqueue(message);

                    this.MessageStore.Remove();
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
