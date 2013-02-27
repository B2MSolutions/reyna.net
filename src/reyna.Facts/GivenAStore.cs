namespace reyna.Facts
{
    using Moq;
    using reyna.Interfaces;
    using Xunit;

    public class GivenAStore
    {
        public GivenAStore()
        {
            this.Repository = new Mock<IRepository>();
            this.Store = new Store();
            this.Store.Repository = this.Repository.Object;
        }

        private Mock<IRepository> Repository { get; set; }

        private Store Store { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Store);
        }

        [Fact]
        public void WhenCallingPutAndRepositoryDoesNotExistShouldCreateItBeforeEnqueueing()
        {
            this.Repository.Setup(r => r.DoesNotExist(It.IsAny<string>())).Returns(true);
            this.Repository.Setup(r => r.Create(It.IsAny<string>()));
            this.Repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var message = new Message(null, null);
            this.Store.Put(message);

            this.Repository.Verify(r => r.DoesNotExist("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Create("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Enqueue(message), Times.Once());
        }

        [Fact]
        public void WhenCallingPutShouldEnqueueRecord()
        {
            this.Repository.Setup(r => r.DoesNotExist(It.IsAny<string>())).Returns(false);
            this.Repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var message = new Message(null, null);
            this.Store.Put(message);

            this.Repository.Verify(r => r.DoesNotExist("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Enqueue(message), Times.Once());
        }
    }
}
