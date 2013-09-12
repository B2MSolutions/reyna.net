namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal abstract class ServiceBase : ThreadWorker
    {
        public ServiceBase(IRepository sourceStore, IWaitHandle waitHandle) : base(waitHandle)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }

            this.SourceStore = sourceStore;
            this.SourceStore.Initialise();

            this.SourceStore.MessageAdded += this.OnMessageAdded;
        }

        protected IRepository SourceStore { get; set; }

        public override void Dispose()
        {
            base.Dispose();

            if (this.SourceStore != null)
            {
                this.SourceStore.MessageAdded -= this.OnMessageAdded;
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
    }
}
