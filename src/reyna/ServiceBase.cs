namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    internal abstract class ServiceBase
    {
        public ServiceBase()
        {
            this.NewMessageAdded = new AutoResetEvent(false);
        }

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
