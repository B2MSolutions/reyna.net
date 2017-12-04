namespace Reyna
{
    using System;
    using System.Net;
    using System.Threading;
    using Reyna.Interfaces;

    public sealed class ReynaService : IReyna
    {
        private const long MinimumStorageLimit = 1867776; // 1Mb 800Kb

        public ReynaService(IReynaLogger logger)
            : this(null, null, logger)
        {
        }

        public ReynaService(bool useNetworkState, IReynaLogger logger)
            : this(null, null, useNetworkState, logger)
        {
        }

        public ReynaService(byte[] password, ICertificatePolicy certificatePolicy, IReynaLogger logger)
            : this(password, certificatePolicy, true, logger)
        {
        }

        public ReynaService(byte[] password, ICertificatePolicy certificatePolicy, bool useNetworkState, IReynaLogger logger)
        {
            this.Password = password;
            this.Logger = logger;
            this.PersistentStore = new SQLiteRepository(logger, password);
            this.HttpClient = new HttpClient(certificatePolicy);
            this.EncryptionChecker = new EncryptionChecker();

            this.ForwardWaitHandle = new AutoResetEventAdapter(false);

            if (useNetworkState)
            {
                this.SystemNotifier = new SystemNotifier();
                this.NetworkWaitHandle = new NamedWaitHandle(false, Reyna.NetworkStateService.NetworkConnectedNamedEvent);
                this.NetworkStateService = new NetworkStateService(this.SystemNotifier, this.NetworkWaitHandle);
            }

            var preferences = new Preferences();
            this.StoreService = new StoreService(this.PersistentStore, logger);
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient, this.NetworkStateService, this.ForwardWaitHandle, Preferences.ForwardServiceTemporaryErrorBackout, Preferences.ForwardServiceMessageBackout, preferences.BatchUpload, logger);
        }

        public static long StorageSizeLimit
        {
            get
            {
                return new Preferences().StorageSizeLimit;
            }
        }

        internal IEncryptionChecker EncryptionChecker { get; set; }

        internal IRepository PersistentStore { get; set; }

        internal IHttpClient HttpClient { get; set; }

        internal IStoreService StoreService { get; set; }

        internal IForward ForwardService { get; set; }

        internal INetworkStateService NetworkStateService { get; set; }

        internal IWaitHandle ForwardWaitHandle { get; set; }

        internal IWaitHandle NetworkWaitHandle { get; set; }

        internal ISystemNotifier SystemNotifier { get; set; }

        internal IReynaLogger Logger { get; set; }

        private byte[] Password { get; set; }

        public static void ResetStorageSizeLimit()
        {
            new Preferences().ResetStorageSizeLimit();
        }

        public static void SetCellularDataBlackout(TimeRange timeRange)
        {
            new Preferences().SetCellularDataBlackout(timeRange);
        }

        public static void ResetCellularDataBlackout()
        {
            new Preferences().ResetCellularDataBlackout();
        }

        public static void SetWlanBlackoutRange(string range)
        {
            new Preferences().SetWlanBlackoutRange(range);
        }

        public static void SetWwanBlackoutRange(string range)
        {
            new Preferences().SetWwanBlackoutRange(range);
        }

        public static void SetRoamingBlackout(bool value)
        {
            new Preferences().SetRoamingBlackout(value);
        }

        public static void SetOnChargeBlackout(bool value)
        {
            new Preferences().SetOnChargeBlackout(value);
        }

        public static void SetOffChargeBlackout(bool value)
        {
            new Preferences().SetOffChargeBlackout(value);
        }

        public static void SetBatchUploadConfiguration(bool value, Uri url, long checkInterval)
        {
            var preferences = new Preferences();
            preferences.SaveBatchUpload(value);
            preferences.SaveBatchUploadUrl(url);
            preferences.SaveBatchUploadCheckInterval(checkInterval);
        }

        public static void SetStorageSizeLimit(IReynaLogger logger, byte[] password, long limit)
        {
            limit = limit < MinimumStorageLimit ? MinimumStorageLimit : limit;
            new Preferences().SetStorageSizeLimit(limit);

            var repository = new SQLiteRepository(logger, password);
            repository.Initialise();
            repository.ShrinkDb(limit);
        }

        public void Start()
        {
            this.EncryptDatabase();
            this.ForwardWaitHandle.Reset();

            this.ForwardService.Start();

            if (this.NetworkStateService != null)
            {
                this.NetworkStateService.Start();
            }
        }

        public void Stop()
        {
            if (this.NetworkStateService != null)
            {
                this.NetworkStateService.Stop();
            }

            this.ForwardService.Stop();
        }

        public void Put(IMessage message)
        {
            this.StoreService.Put(message);
        }

        public void ResumeForwardService()
        {
            if (this.ForwardService != null)
            {
                this.ForwardService.Resume();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.NetworkStateService != null)
                {
                    this.NetworkStateService.Dispose();
                }

                if (this.ForwardService != null)
                {
                    this.ForwardService.Dispose();
                }
            }
        }

        private void EncryptDatabase()
        {
            if (this.Password == null || this.Password.Length == 0)
            {
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (this.EncryptionChecker.DbEncrypted())
                    {
                        return;
                    }
                    
                    this.EncryptionChecker.EncryptDb(this.Password);
                    return;
                }
                catch (Exception exception)
                {
                    this.Logger.Err("ReynaService.EncryptDatabase. Error {0}", exception.StackTrace);
                }

                Reyna.Sleep.Wait(2);
            }
        }
    }
}
