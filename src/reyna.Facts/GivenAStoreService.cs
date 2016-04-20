namespace Reyna.Facts
{
    using System;
    using System.Threading;
    using Microsoft.Win32;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAStoreService
    {
        public GivenAStoreService()
        {
            this.VolatileStore = new InMemoryQueue();
            this.PersistentStore = new Mock<IRepository>();
            this.WaitHandle = new AutoResetEventAdapter(false);
            this.Logger = new Mock<ILogger>();

            this.PersistentStore.Setup(r => r.Add(It.IsAny<IMessage>()));

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore.Object, this.WaitHandle, this.Logger.Object);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        private IRepository VolatileStore { get; set; }

        private Mock<IRepository> PersistentStore { get; set; }

        private Mock<ILogger> Logger { get; set; }

        private IWaitHandle WaitHandle { get; set; }

        private StoreService StoreService { get; set; }

        [Fact]
        public void WhenCallingStartAndMessageAddedShouldCallPutOnRepository()
        {
            this.StoreService.Start();

            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            Thread.Sleep(500);
            this.StoreService.Stop();

            Assert.Null(this.VolatileStore.Get());
            this.PersistentStore.Verify(r => r.Add(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedAndStorageSizeLimitExistsShouldCallAddWithLimitOnRepository()
        {
            using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Reyna"))
            {
                ReynaService.SetStorageSizeLimit(null, 2000000);
                this.StoreService.Start();

                this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
                Thread.Sleep(200);

                Assert.Null(this.VolatileStore.Get());
                this.PersistentStore.Verify(r => r.Add(It.IsAny<IMessage>(), 2000000), Times.Once());
            }

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenCallingStopOnStoreThatHasntStartedShouldNotThrow()
        {
            this.StoreService.Stop();
        }

        [Fact]
        public void WhenCallingDisposeShouldNotThrow()
        {
            this.StoreService.Dispose();
        }

        [Fact]
        public void WhenCallingStartStopDisposeShouldNotThrow()
        {
            this.StoreService.Start();
            Thread.Sleep(50);

            this.StoreService.Stop();
            Thread.Sleep(50);
            
            this.StoreService.Dispose();
        }

        [Fact]
        public void WhenConstructingWithBothNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(null, null, this.WaitHandle, this.Logger.Object));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullMessageStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(null, new Mock<IRepository>().Object, this.WaitHandle, this.Logger.Object));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullRepositoryParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(new InMemoryQueue(), null, this.WaitHandle, this.Logger.Object));
            Assert.Equal("targetStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullWaitHandleStateParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(new Mock<IRepository>().Object, new Mock<IRepository>().Object, null, this.Logger.Object));
            Assert.Equal("waitHandle", exception.ParamName);
        }

        [Fact]
        public void WhenAddingMessageAndThrowsShouldSucceedNextTime()
        {
            this.StoreService.Start();

            var exception = new InvalidOperationException("Error");
            
            this.PersistentStore.Setup(s => s.Add(It.IsAny<IMessage>())).Throws(exception);
            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            
            Thread.Sleep(500);
            this.PersistentStore.Setup(s => s.Add(It.IsAny<IMessage>()));
            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            
            Thread.Sleep(500);
            this.StoreService.Stop();

            Assert.Null(this.VolatileStore.Get());
            this.PersistentStore.Verify(r => r.Add(It.IsAny<IMessage>()), Times.AtLeast(2));
            this.Logger.Verify(l => l.Err("StoreService.ThreadStart. Error {0}", exception.ToString()), Times.AtLeast(1));
        }
    }
}
