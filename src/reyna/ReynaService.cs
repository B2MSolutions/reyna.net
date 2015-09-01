﻿namespace Reyna
{
    using System;
    using System.Net;
    using Microsoft.Win32;
    using Reyna.Interfaces;

    public sealed class ReynaService : IReyna
    {
        private const long MinimumStorageLimit = 1867776; // 1Mb 800Kb

        public ReynaService() : this(null, null)
        {
        }

        public ReynaService(byte[] password, ICertificatePolicy certificatePolicy)
        {
            this.Password = password;
            this.VolatileStore = new InMemoryQueue();
            this.PersistentStore = new SQLiteRepository(password);
            this.HttpClient = new HttpClient(certificatePolicy);
            this.EncryptionChecker = new EncryptionChecker();

            this.StoreWaitHandle = new AutoResetEventAdapter(false);
            this.ForwardWaitHandle = new AutoResetEventAdapter(false);
            this.NetworkWaitHandle = new NamedWaitHandle(false, Reyna.NetworkStateService.NetworkConnectedNamedEvent);

            this.SystemNotifier = new SystemNotifier();

            this.NetworkStateService = new NetworkStateService(this.SystemNotifier, this.NetworkWaitHandle);

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore, this.StoreWaitHandle);
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient, this.NetworkStateService, this.ForwardWaitHandle, Preferences.ForwardServiceTemporaryErrorBackout, Preferences.ForwardServiceMessageBackout);
        }

        public static long StorageSizeLimit
        {
            get
            {
                return Preferences.StorageSizeLimit;
            }
        }

        internal IEncryptionChecker EncryptionChecker { get; set; }

        internal IRepository VolatileStore { get; set; }

        internal IRepository PersistentStore { get; set; }

        internal IHttpClient HttpClient { get; set; }

        internal IService StoreService { get; set; }

        internal IService ForwardService { get; set; }

        internal INetworkStateService NetworkStateService { get; set; }

        internal IWaitHandle StoreWaitHandle { get; set; }

        internal IWaitHandle ForwardWaitHandle { get; set; }

        internal IWaitHandle NetworkWaitHandle { get; set; }

        internal ISystemNotifier SystemNotifier { get; set; }

        private byte[] Password { get; set; }

        public static void ResetStorageSizeLimit()
        {
            Preferences.ResetStorageSizeLimit();
        }

        public static void SetStorageSizeLimit(byte[] password, long limit)
        {
            limit = limit < MinimumStorageLimit ? MinimumStorageLimit : limit;
            Preferences.SetStorageSizeLimit(limit);

            var repository = new SQLiteRepository(password);
            repository.Initialise();
            repository.ShrinkDb(limit);
        }

        public static void SetCellularDataBlackout(TimeRange timeRange)
        {
            Preferences.SetCellularDataBlackout(timeRange);
        }

        public static void ResetCellularDataBlackout()
        {
            Preferences.ResetCellularDataBlackout();
        }

        public void Start()
        {
            if (this.Password != null && this.Password.Length > 0)
            {
                if (!this.EncryptionChecker.DbEncrypted())
                {
                    this.EncryptionChecker.EncryptDb(this.Password);
                }
            }

            this.StoreService.Start();
            this.ForwardService.Start();
            this.NetworkStateService.Start();
        }

        public void Stop()
        {
            this.NetworkStateService.Stop();
            this.ForwardService.Stop();
            this.StoreService.Stop();
        }

        public void Put(IMessage message)
        {
            this.VolatileStore.Add(message);
        }

        public void Dispose()
        {
            if (this.NetworkStateService != null)
            {
                this.NetworkStateService.Dispose();
            }

            if (this.ForwardService != null)
            {
                this.ForwardService.Dispose();
            }

            if (this.StoreService != null)
            {
                this.StoreService.Dispose();
            }
        }

        internal static void SetWlanBlackoutRange(string range)
        {
            Preferences.SetWlanBlackoutRange(range);
        }

        internal static void SetWwanBlackoutRange(string range)
        {
            Preferences.SetWwanBlackoutRange(range);
        }

        internal static void SetRoamingBlackout(bool value)
        {
            Preferences.SetRoamingBlackout(value);
        }

        internal static void SetOnChargeBlackout(bool value)
        {
            Preferences.SetOnChargeBlackout(value);            
        }

        internal static void SetOffChargeBlackout(bool value)
        {
            Preferences.SetOffChargeBlackout(value);            
        }
    }
}
