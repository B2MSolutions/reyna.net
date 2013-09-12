namespace Reyna
{
    using System;
    using System.Threading;
    using Reyna.Interfaces;

    internal sealed class StoreService : ServiceBase
    {
        public StoreService(IRepository sourceStore, IRepository targetStore, IWaitHandle waitHandle) : base(sourceStore, waitHandle, false)
        {
            if (targetStore == null)
            {
                throw new ArgumentNullException("targetStore");
            }

            this.TargetStore = targetStore;
            this.TargetStore.Initialise();
        }

        private IRepository TargetStore { get; set; }

        protected override void ThreadStart()
        {
            while (!this.Terminate)
            {
                this.WaitHandle.WaitOne();
                IMessage message = null;

                while ((message = this.SourceStore.Get()) != null)
                {
                    this.TargetStore.Add(message);

                    this.SourceStore.Remove();
                }

                this.WaitHandle.Reset();
            }
        }
    }
}
