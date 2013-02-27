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
        public void WhenCallingPutAndRepositoryDoesNotExistShouldCreateItBeforeInserting()
        {
            this.Repository.Setup(r => r.DoesExist(It.IsAny<string>())).Returns(false);
            this.Repository.Setup(r => r.Create(It.IsAny<string>()));
            this.Repository.Setup(r => r.Insert(It.IsAny<IMessage>()));

            var message = new Message();
            this.Store.Put(message);

            this.Repository.Verify(r => r.DoesExist("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Create("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Insert(message), Times.Once());
        }

        [Fact]
        public void WhenCallingPutShouldInsertRecord()
        {
            this.Repository.Setup(r => r.DoesExist(It.IsAny<string>())).Returns(true);
            this.Repository.Setup(r => r.Insert(It.IsAny<IMessage>()));

            var message = new Message();
            this.Store.Put(message);

            this.Repository.Verify(r => r.DoesExist("reyna.db"), Times.Once());
            this.Repository.Verify(r => r.Insert(message), Times.Once());
        }
    }
}
