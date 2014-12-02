namespace Reyna.Facts
{
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAReynaService
    {
        public GivenAReynaService()
        {
            this.VolatileStore = new Mock<IRepository>();
            this.StoreService = new Mock<IService>();
            this.ForwardService = new Mock<IService>();
            this.NetworkStateService = new Mock<INetworkStateService>();

            this.ReynaService = new ReynaService();
            this.ReynaService.VolatileStore = this.VolatileStore.Object;
            this.ReynaService.StoreService = this.StoreService.Object;
            this.ReynaService.ForwardService = this.ForwardService.Object;
            this.ReynaService.NetworkStateService = this.NetworkStateService.Object;
        }

        private Mock<IRepository> VolatileStore { get; set; }

        private Mock<IService> StoreService { get; set; }

        private Mock<IService> ForwardService { get; set; }
        
        private Mock<INetworkStateService> NetworkStateService { get; set; }

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
            var reynaService = new ReynaService(password);

            Assert.Equal(password, ((SQLiteRepository)reynaService.PersistentStore).Password); 
        }

        [Fact]
        public void WhenConstructingWithoutPasswordShouldNotPassAnyPasswordToSQLiteRepository()
        {
            var reynaService = new ReynaService();

            Assert.Null(((SQLiteRepository)reynaService.PersistentStore).Password);
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
    }
}
