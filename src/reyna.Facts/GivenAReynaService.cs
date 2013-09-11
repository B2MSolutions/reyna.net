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

            this.ReynaService = new ReynaService();
            this.ReynaService.VolatileStore = this.VolatileStore.Object;
            this.ReynaService.StoreService = this.StoreService.Object;
        }

        private Mock<IRepository> VolatileStore { get; set; }

        private Mock<IService> StoreService { get; set; }

        private ReynaService ReynaService { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ReynaService);
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

            this.ReynaService.Start();

            this.StoreService.Verify(s => s.Start(), Times.Once());
        }

        [Fact]
        public void WhenCallingStopShouldStopStoreService()
        {
            this.StoreService.Setup(s => s.Stop());

            this.ReynaService.Stop();

            this.StoreService.Verify(s => s.Stop(), Times.Once());
        }

        [Fact]
        public void WhenCallingDisposeShouldCallDisposeOnStoreService()
        {
            this.StoreService.Setup(s => s.Dispose());

            this.ReynaService.Dispose();

            this.StoreService.Verify(s => s.Dispose(), Times.Once());
        }
    }
}
