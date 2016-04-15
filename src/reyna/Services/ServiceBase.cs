namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal abstract class ServiceBase : ThreadWorker
    {
        public ServiceBase(IRepository sourceStore, IWaitHandle waitHandle, bool runOnStart, ILogger logger)
            : base(waitHandle, runOnStart)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }

            this.SourceStore = sourceStore;
            this.Logger = logger;
            this.SourceStore.Initialise();

            this.SourceStore.MessageAdded += this.OnMessageAdded;
        }

        protected IRepository SourceStore { get; set; }

        protected ILogger Logger { get; set; }

        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void OnMessageAdded(object sender, EventArgs e)
        {
            if (this.Terminate)
            {
                return;
            }

            this.SignalWorkToDo();
        }

        protected virtual void OnDispose()
        {
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose();

                this.OnDispose();

                if (this.SourceStore != null)
                {
                    this.SourceStore.MessageAdded -= this.OnMessageAdded;
                }
            }
        }
    }
}
