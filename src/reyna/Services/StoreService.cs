namespace Reyna
{
    using System;
    using Reyna.Interfaces;

    internal sealed class StoreService : IStoreService
    {
        public StoreService(IRepository targetStore, IReynaLogger logger)
        {
            if (targetStore == null)
            {
                throw new ArgumentNullException("targetStore");
            }

            this.TargetStore = targetStore;
            this.Logger = logger;
            
            this.TargetStore.Initialise();
        }

        private IRepository TargetStore { get; set; }

        private IReynaLogger Logger { get; set; }

        public void Put(IMessage message)
        {
            long storageSizeLimit = new Preferences().StorageSizeLimit;
            this.Logger.Debug("Reyna.StoreService.Put {0} -> {1} ({3})", message.Id, message.Url.AbsolutePath, storageSizeLimit);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (storageSizeLimit == -1)
                    {
                        this.TargetStore.Add(message);
                    }
                    else
                    {
                        this.TargetStore.Add(message, storageSizeLimit);
                    }

                    return;
                }
                catch (Exception exception)
                {
                    this.Logger.Err("StoreService.ThreadStart. Error {0}", exception.ToString());
                }

                Reyna.Sleep.Wait(1);
            }
        }
    }
}
