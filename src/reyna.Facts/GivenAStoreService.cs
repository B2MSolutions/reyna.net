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

            this.PersistentStore.Setup(r => r.Add(It.IsAny<IMessage>()));

            this.StoreService = new StoreService(this.VolatileStore, this.PersistentStore.Object, this.WaitHandle);
        }

        private IRepository VolatileStore { get; set; }

        private Mock<IRepository> PersistentStore { get; set; }

        private IWaitHandle WaitHandle { get; set; }

        private StoreService StoreService { get; set; }

        [Fact]
        public void WhenCallingStartAndMessageAddedShouldCallPutOnRepository()
        {
            this.StoreService.Start();

            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            Thread.Sleep(200);

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

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna");
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedThenImmediatelyStopShouldNotCallPutOnRepository()
        {
            this.StoreService.Start();
            Thread.Sleep(50);

            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            this.StoreService.Stop();
            Thread.Sleep(200);

            this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            Thread.Sleep(200);

            Assert.NotNull(this.VolatileStore.Get());
            this.PersistentStore.Verify(r => r.Add(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact(Skip = "true")]
        public void WhenCallingStartAndStopRapidlyWhilstAddingMessagesShouldNotCallPutOnRepository()
        {
            var messageAddingThread = new Thread(new ThreadStart(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        this.VolatileStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
                        Thread.Sleep(100);
                    }
                }));

            messageAddingThread.Start();
            Thread.Sleep(100);

            for (int k = 0; k < 10; k++)
            {
                this.StoreService.Start();
                Thread.Sleep(50);

                this.StoreService.Stop();
                Thread.Sleep(200);
            }

            Thread.Sleep(1000);

            Assert.Null(this.VolatileStore.Get());
            this.PersistentStore.Verify(r => r.Add(It.IsAny<IMessage>()), Times.Exactly(10));
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
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(null, null, this.WaitHandle));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullMessageStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(null, new Mock<IRepository>().Object, this.WaitHandle));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullRepositoryParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(new InMemoryQueue(), null, this.WaitHandle));
            Assert.Equal("targetStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullWaitHandleStateParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(new Mock<IRepository>().Object, new Mock<IRepository>().Object, null));
            Assert.Equal("waitHandle", exception.ParamName);
        }
    }
}
