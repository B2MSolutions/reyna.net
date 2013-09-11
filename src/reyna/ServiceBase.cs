namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal abstract class ServiceBase : IService
    {
        public ServiceBase(IRepository sourceStore)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }

            this.SourceStore = sourceStore;
            this.SourceStore.Initialise();

            this.NewMessageAdded = new AutoResetEvent(false);

            this.SourceStore.MessageAdded += this.OnMessageAdded;
        }

        protected IRepository SourceStore { get; set; }

        protected bool Terminate { get; set; }

        protected AutoResetEvent NewMessageAdded { get; set; }

        private Thread WorkingThread { get; set; }

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

            if (this.SourceStore != null)
            {
                this.SourceStore.MessageAdded -= this.OnMessageAdded;
            }
        }

        protected void WaitForWorkingThreadToFinish()
        {
            if (this.WorkingThread != null)
            {
                this.WorkingThread.Join();
            }
        }

        protected void OnMessageAdded(object sender, EventArgs e)
        {
            if (this.Terminate)
            {
                return;
            }

            this.NewMessageAdded.Set();
        }

        protected abstract void ThreadStart();
    }
}
