namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal abstract class ServiceBase : IService
    {
        private volatile bool terminate;

        public ServiceBase(IRepository sourceStore)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }

            this.SourceStore = sourceStore;
            this.SourceStore.Initialise();

            this.DoWorkEvent = new AutoResetEvent(false);

            this.SourceStore.MessageAdded += this.OnMessageAdded;
        }

        protected IRepository SourceStore { get; set; }

        protected bool Terminate
        {
            get
            {
                return this.terminate;
            }

            set
            {
                this.terminate = value;
            }
        }

        protected AutoResetEvent DoWorkEvent { get; set; }

        private Thread WorkingThread { get; set; }

        public void Start()
        {
            this.SetWorkerThreadStartState();
            this.CreateWorkerThread();
            this.SignalWorkToDo();
        }

        public void Stop()
        {
            this.SetWorkerThreadStopState();
            this.SignalWorkToDo();
            this.WaitForWorkerThreadToStop();
        }

        public void Dispose()
        {
            this.Stop();

            if (this.SourceStore != null)
            {
                this.SourceStore.MessageAdded -= this.OnMessageAdded;
            }
        }

        protected void WaitForWorkerThreadToStop()
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

            this.SignalWorkToDo();
        }

        protected abstract void ThreadStart();

        private void CreateWorkerThread()
        {
            this.WorkingThread = new Thread(this.ThreadStart);
            this.WorkingThread.Start();
        }

        private void SignalWorkToDo()
        {
            this.DoWorkEvent.Set();
        }

        private void SetWorkerThreadStartState()
        {
            this.Terminate = false;
        }

        private void SetWorkerThreadStopState()
        {
            this.Terminate = true;
        }
    }
}
