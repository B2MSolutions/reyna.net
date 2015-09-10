namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal abstract class ThreadWorker : IService
    {
        private volatile bool terminate;

        public ThreadWorker(IWaitHandle waitHandle, bool runOnStart)
        {
            if (waitHandle == null)
            {
                throw new ArgumentNullException("waitHandle");
            }

            this.WaitHandle = waitHandle;
            this.RunOnStart = runOnStart;
        }

        protected IWaitHandle WaitHandle { get; set; }

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

        private Thread WorkingThread { get; set; }

        private bool RunOnStart { get; set; }

        public virtual void Start()
        {
            this.SetWorkerThreadStartState();
            this.CreateWorkerThread();
            
            if (!this.RunOnStart)
            {
                return;
            }

            this.SignalWorkToDo();
        }

        public virtual void Stop()
        {
            this.SetWorkerThreadStopState();
            this.SignalWorkToDo();
            this.WaitForWorkerThreadToStop();
        }

        public virtual void Dispose()
        {
            this.Stop();
        }

        protected void SignalWorkToDo()
        {
            this.WaitHandle.Set();
        }

        protected void WaitForWorkerThreadToStop()
        {
            if (this.WorkingThread != null)
            {
                this.WorkingThread.Join();
            }
        }

        protected abstract void ThreadStart();

        private void SetWorkerThreadStartState()
        {
            this.Terminate = false;
        }

        private void SetWorkerThreadStopState()
        {
            this.Terminate = true;
        }

        private void CreateWorkerThread()
        {
            this.WorkingThread = new Thread(this.ThreadStart);
            this.WorkingThread.Start();
        }
    }
}
