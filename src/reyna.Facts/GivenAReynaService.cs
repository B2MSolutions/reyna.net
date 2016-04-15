namespace Reyna.Facts
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;
    using Xunit.Extensions;

    public class GivenAReynaService
    {
        public GivenAReynaService()
        {
            this.VolatileStore = new Mock<IRepository>();
            this.StoreService = new Mock<IService>();
            this.ForwardService = new Mock<IForward>();
            this.NetworkStateService = new Mock<INetworkStateService>();
            this.Logger = new Mock<ILogger>();

            this.ReynaService = new ReynaService(this.Logger.Object);
            this.ReynaService.VolatileStore = this.VolatileStore.Object;
            this.ReynaService.StoreService = this.StoreService.Object;
            this.ReynaService.ForwardService = this.ForwardService.Object;
            this.ReynaService.NetworkStateService = this.NetworkStateService.Object;

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        private Mock<IRepository> VolatileStore { get; set; }

        private Mock<IService> StoreService { get; set; }

        private Mock<IForward> ForwardService { get; set; }
        
        private Mock<INetworkStateService> NetworkStateService { get; set; }
        
        private Mock<ILogger> Logger { get; set; }

        private ReynaService ReynaService { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ReynaService);
        }

        [Fact]
        public void WhenConstructingAndReceivedPasswordShouldPassPasswordToSQLiteRepository()
        {
            var password = new byte[] { 0xFF, 0xAA, 0xCC, 0xCC };
            var reynaService = new ReynaService(password, null, this.Logger.Object);

            Assert.Equal(password, ((SQLiteRepository)reynaService.PersistentStore).Password); 
        }

        [Fact]
        public void WhenConstructingWithoutPasswordShouldNotPassAnyPasswordToSQLiteRepository()
        {
            var reynaService = new ReynaService(this.Logger.Object);

            Assert.Null(((SQLiteRepository)reynaService.PersistentStore).Password);
        }

        [Fact]
        public void WhenConstructingWithUseNetworkStateIsFalseShouldNotUseNetworkStateService()
        {
            var reynaService = new ReynaService(false, this.Logger.Object);
            Assert.Null(reynaService.NetworkStateService);
            reynaService.Dispose();
        }

        [Fact]
        public void StartStopDisposeShouldWork()
        {
            var reynaService = new ReynaService(false, this.Logger.Object);
            Assert.Null(reynaService.NetworkStateService);
            reynaService.Start();
            System.Threading.Thread.Sleep(1000);
            reynaService.Stop();
            reynaService.Stop();
            reynaService.Stop();
            reynaService.Dispose();
            reynaService.Dispose();
            reynaService.Dispose();
        }

        [Fact]
        public void WhenConstructingWithUseNetworkStateIsFalseAndHasOtherArgsShouldNotUseNetworkStateService()
        {
            var password = new byte[] { 0xFF, 0xAA, 0xCC, 0xCC };
            var reynaService = new ReynaService(password, null, false, this.Logger.Object);
            Assert.Null(reynaService.NetworkStateService);
            Assert.Equal(password, ((SQLiteRepository)reynaService.PersistentStore).Password);
            reynaService.Dispose();
        }

        [Fact]
        public void WhenConstructingWithUseNetworkStateIsTrueShouldUseNetworkStateService()
        {
            var reynaService = new ReynaService(null, null, true, this.Logger.Object);
            Assert.NotNull(reynaService.NetworkStateService);
            reynaService.Dispose();
        }

        [Fact]
        public void WhenCallingPutShouldAddMessage()
        {
            this.VolatileStore.Setup(r => r.Add(It.IsAny<IMessage>()));

            var message = new Message(null, null);
            this.ReynaService.Put(message);

            this.VolatileStore.Verify(r => r.Add(message), Times.Once());
        }

        [Fact]
        public void WhenCallingStartShouldStartStoreService()
        {
            this.StoreService.Setup(s => s.Start());
            this.ForwardService.Setup(f => f.Start());
            this.NetworkStateService.Setup(f => f.Start());

            this.ReynaService.Start();

            this.StoreService.Verify(s => s.Start(), Times.Once());
            this.ForwardService.Verify(f => f.Start(), Times.Once());
            this.NetworkStateService.Verify(f => f.Start(), Times.Once());
        }

        [Fact]
        public void WhenCallingStopShouldStopStoreService()
        {
            this.StoreService.Setup(s => s.Stop());
            this.ForwardService.Setup(f => f.Stop());
            this.NetworkStateService.Setup(f => f.Stop());

            this.ReynaService.Stop();

            this.StoreService.Verify(s => s.Stop(), Times.Once());
            this.ForwardService.Verify(f => f.Stop(), Times.Once());
            this.NetworkStateService.Verify(f => f.Stop(), Times.Once());
        }

        [Fact]
        public void WhenCallingDisposeShouldCallDisposeOnStoreService()
        {
            this.StoreService.Setup(s => s.Dispose());
            this.ForwardService.Setup(s => s.Dispose());
            this.NetworkStateService.Setup(s => s.Dispose());

            this.ReynaService.Dispose();

            this.StoreService.Verify(s => s.Dispose(), Times.Once());
            this.ForwardService.Verify(f => f.Dispose(), Times.Once());
            this.NetworkStateService.Verify(f => f.Dispose(), Times.Once());
        }

        [Fact]
        public void WhenGettingForwardServiceTemporaryErrorBackoutAndNoRegistryKeyShouldReturnDefault5Minutes()
        {
            Assert.Equal(300000, Preferences.ForwardServiceTemporaryErrorBackout);
        }

        [Fact]
        public void WhenGettingForwardServiceTemporaryErrorBackoutAndRegistryKeyExistsShouldReturnExpected()
        {
            using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Reyna"))
            {
                key.SetValue("TemporaryErrorBackout", 100);
            }

            Assert.Equal(100, Preferences.ForwardServiceTemporaryErrorBackout);

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenGettingForwardServiceMessageBackoutAndNoRegistryKeyShouldReturnDefault5Minutes()
        {
            Assert.Equal(1000, Preferences.ForwardServiceMessageBackout);
        }

        [Fact]
        public void WhenGettingForwardServiceMessageBackoutAndRegistryKeyExistsShouldReturnExpected()
        {
            using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Reyna"))
            {
                key.SetValue("MessageBackout", 10);
            }

            Assert.Equal(10, Preferences.ForwardServiceMessageBackout);

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenSettingStorageLimitShouldSaveStorageLimit()
        {
            ReynaService.SetStorageSizeLimit(null, 3145728);
            Assert.Equal(3145728, ReynaService.StorageSizeLimit);

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenGettingStorageLimitShouldSaveStorageLimit()
        {
            ReynaService.SetStorageSizeLimit(null, 3145728);
            Assert.Equal(3145728, ReynaService.StorageSizeLimit);

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenSettingStorageLimitShouldInitializeReyna()
        {
            File.Delete("reyna.db");
            ReynaService.SetStorageSizeLimit(null, 3145728);
            Assert.True(File.Exists("reyna.db"));
        }
        
        [Theory]
        [InlineData(-42)]
        [InlineData(0)]
        [InlineData(42)]
        public void WhenSettingStorageLimitShouldSetToMinimumValue(long value) 
        {
            ReynaService.SetStorageSizeLimit(null, value);
            Assert.Equal(1867776, ReynaService.StorageSizeLimit); // 1867776 - min value, 1.8 Mb

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }
        
        [Fact]
        public void WhenResettingsStorageLimitShouldDeleteIt()
        {
            ReynaService.SetStorageSizeLimit(null, 100);
            ReynaService.ResetStorageSizeLimit();
            Assert.Equal(-1, ReynaService.StorageSizeLimit);
        }

        [Fact]
        public void WhenResettingsStorageLimitAndRegistryKeyNotExistsShouldNotThrows()
        {
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
            
            ReynaService.ResetStorageSizeLimit();
        }

        [Fact]
        public void WhenSetCellularDataBlackoutShouldStoreIt()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            ReynaService.SetCellularDataBlackout(range);

            TimeRange timeRange = new Preferences().CellularDataBlackout;

            Assert.Equal(range.From.MinuteOfDay, timeRange.From.MinuteOfDay);
            Assert.Equal(range.To.MinuteOfDay, timeRange.To.MinuteOfDay);
        }

        [Fact]
        public void WhenResetCellularDataBlackoutThenGetCellularDataBlackoutShouldReturnNull()
        {
            TimeRange range = new TimeRange(new Time(11, 00), new Time(12, 01));
            ReynaService.ResetCellularDataBlackout();

            TimeRange timeRange = new Preferences().CellularDataBlackout;

            Assert.Null(timeRange);
        }

        [Fact]
        public void WhenSetWlanBlackoutRangeShouldStoreIt()
        {
            string range = "00:00-00:10";
            ReynaService.SetWlanBlackoutRange(range);

            string actual = new Preferences().WlanBlackoutRange;

            Assert.Equal(range, actual);
        }

        [Fact]
        public void WhenSetWwanBlackoutRangeShouldStoreIt()
        {
            string range = "00:00-00:10";
            ReynaService.SetWwanBlackoutRange(range);

            string actual = new Preferences().WwanBlackoutRange;

            Assert.Equal(range, actual);
        }

        [Fact]
        public void WhenSetRoamingBlackoutShouldStoreIt()
        {
            ReynaService.SetRoamingBlackout(false);

            bool actual = new Preferences().RoamingBlackout;

            Assert.False(actual);
        }

        [Fact]
        public void WhenSetOnChargeBlackoutShouldStoreIt()
        {
            ReynaService.SetOnChargeBlackout(false);

            bool actual = new Preferences().OnChargeBlackout;

            Assert.False(actual);
        }

        [Fact]
        public void WhenSetOffChargeBlackoutShouldStoreIt()
        {
            ReynaService.SetOffChargeBlackout(false);

            bool actual = new Preferences().OffChargeBlackout;

            Assert.False(actual);
        }

        [Fact]
        public void WhenCallingResumeForwardServiceShouldCallResume()
        {
            this.ForwardService.Setup(f => f.Resume());

            this.ReynaService.ResumeForwardService();

            this.ForwardService.Verify(f => f.Resume(), Times.Once());
        }

        [Fact]
        public void WhenSetBatchUploadConfigurationShouldSaveBatchUploadValues()
        {
            ReynaService.SetBatchUploadConfiguration(true, new Uri("http://www.post.com"), 74);

            var preferences = new Preferences();
            Assert.True(preferences.BatchUpload);
            Assert.Equal("http://www.post.com/", preferences.BatchUploadUrl.ToString());
            Assert.Equal(74, preferences.BatchUploadCheckInterval);

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenSetBatchUploadConfigurationAndNotSavedShouldReturnDefaults()
        {
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);

            var preferences = new Preferences();
            Assert.True(preferences.BatchUpload);
            Assert.Null(preferences.BatchUploadUrl);
            Assert.Equal(21600000, preferences.BatchUploadCheckInterval);
        }
    }
}
