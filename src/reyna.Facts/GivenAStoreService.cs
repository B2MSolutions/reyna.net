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
            this.PersistentStore = new Mock<IRepository>();
            this.WaitHandle = new AutoResetEventAdapter(false);
            this.Logger = new Mock<IReynaLogger>();

            this.PersistentStore.Setup(r => r.Add(It.IsAny<IMessage>()));

            this.StoreService = new StoreService(this.PersistentStore.Object, this.Logger.Object);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        private IRepository VolatileStore { get; set; }

        private Mock<IRepository> PersistentStore { get; set; }

        private Mock<IReynaLogger> Logger { get; set; }

        private IWaitHandle WaitHandle { get; set; }

        private StoreService StoreService { get; set; }

        [Fact]
        public void WhenCallingStartAndMessageAddedShouldCallPutOnRepository()
        {
            var message = new Message(new Uri("http://www.google.com"), string.Empty);
            
            this.StoreService.Put(message);

            this.PersistentStore.Verify(r => r.Add(message), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedAndStorageSizeLimitExistsShouldCallAddWithLimitOnRepository()
        {
            using (var key = Registry.LocalMachine.CreateSubKey(@"Software\Reyna"))
            {
                var message = new Message(new Uri("http://www.google.com"), string.Empty);

                new ReynaService(new ReynaNullLogger()).SetStorageSizeLimit(null, 2000000);
                 this.StoreService.Put(message);

                this.PersistentStore.Verify(r => r.Add(message, 2000000), Times.Once());
            }

            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        [Fact]
        public void WhenConstructingWithNullRepositoryParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new StoreService(null, this.Logger.Object));
            Assert.Equal("targetStore", exception.ParamName);
        }

        [Fact]
        public void WhenAddingMessageAndThrowsShouldLogError()
        {
            var message = new Message(new Uri("http://www.google.com"), string.Empty);
            var exception = new InvalidOperationException("Error");

            this.PersistentStore.Setup(s => s.Add(message)).Throws(exception);
            this.StoreService.Put(message);
            this.PersistentStore.Verify(r => r.Add(message), Times.AtLeast(1));
            this.Logger.Verify(l => l.Err("StoreService.ThreadStart. Error {0}", exception.ToString()), Times.AtLeast(1));
        }

        [Fact]
        public void WhenAddingMessageFailShouldRetry()
        {
            var message = new Message(new Uri("http://www.google.com"), string.Empty);
            var exception = new InvalidOperationException("Error");

            var count = 0;
            this.PersistentStore.Setup(s => s.Add(message))
                .Callback(() =>
                {
                    count++;
                    if (count % 10 != 0)
                    {
                        throw exception;
                    }
                });
            
            this.StoreService.Put(message);
            this.PersistentStore.Verify(r => r.Add(message), Times.Exactly(10));
            this.Logger.Verify(l => l.Err("StoreService.ThreadStart. Error {0}", exception.ToString()), Times.Exactly(9));
        }
    }
}
