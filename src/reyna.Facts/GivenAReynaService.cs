namespace Reyna.Facts
{
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAReynaService
    {
        public GivenAReynaService()
        {
            this.Repository = new Mock<IRepository>();
            this.Forward = new Mock<IForward>();

            this.ReynaService = new ReynaService();
            this.ReynaService.Repository = this.Repository.Object;
            this.ReynaService.Forward = this.Forward.Object;
        }

        private Mock<IRepository> Repository { get; set; }

        private Mock<IForward> Forward { get; set; }

        private ReynaService ReynaService { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ReynaService);
        }

        [Fact]
        public void WhenCallingPutAndRepositoryDoesNotExistShouldCreateItBeforeEnqueueing()
        {
            this.Repository.SetupGet(r => r.DoesNotExist).Returns(true);
            this.Repository.Setup(r => r.Create());
            this.Repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var message = new Message(null, null);
            this.ReynaService.Put(message);

            this.Repository.VerifyGet(r => r.DoesNotExist, Times.Once());
            this.Repository.Verify(r => r.Create(), Times.Once());
            this.Repository.Verify(r => r.Enqueue(message), Times.Once());
        }

        [Fact]
        public void WhenCallingPutShouldEnqueueRecord()
        {
            this.Repository.SetupGet(r => r.DoesNotExist).Returns(false);
            this.Repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var message = new Message(null, null);
            this.ReynaService.Put(message);

            this.Repository.VerifyGet(r => r.DoesNotExist, Times.Once());
            this.Repository.Verify(r => r.Enqueue(message), Times.Once());
        }

        [Fact]
        public void WhenCallingPutShouldForwardMessages()
        {
            this.Forward.Setup(f => f.Send());

            var message = new Message(null, null);
            this.ReynaService.Put(message);

            this.Forward.Verify(f => f.Send(), Times.Once());
        }
    }
}
