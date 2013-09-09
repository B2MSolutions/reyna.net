namespace Reyna.Facts
{
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAForward
    {
        public GivenAForward()
        {
            this.Repository = new Mock<IRepository>();
            
            this.HttpClient = new Mock<IHttpClient>();

            this.Forward = new Forward(this.Repository.Object, this.HttpClient.Object);
        }

        private Mock<IRepository> Repository { get; set; }

        private Mock<IHttpClient> HttpClient { get; set; }

        private IForward Forward { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.Forward);
        }

        [Fact]
        public void WhenCallingSendAndSucceedsShouldRemoveMessageFromQueue()
        {
            var messageCounter = 0;
            var message = new Message(null, null);

            this.Repository.Setup(r => r.Peek())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 2 ? message : (IMessage)null);

            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.Ok);
            this.Repository.Setup(r => r.Dequeue()).Returns(message);

            this.Forward.Send();

            this.Repository.Verify(r => r.Peek(), Times.Exactly(2));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Once());
            this.Repository.Verify(r => r.Dequeue(), Times.Once());
        }

        [Fact]
        public void WhenCallingSendAndNoMessageShouldDoNothing()
        {
            this.Repository.Setup(r => r.Peek()).Returns((IMessage)null);

            this.Forward.Send();

            this.Repository.Verify(r => r.Peek(), Times.Once());
            this.HttpClient.Verify(hc => hc.Post(It.IsAny<IMessage>()), Times.Never());
            this.Repository.Verify(r => r.Dequeue(), Times.Never());
        }

        [Fact]
        public void WhenCallingSendAndHasTemporaryErrorShouldNotRemoveMessageFromQueue()
        {
            var message = new Message(null, null);

            this.Repository.Setup(r => r.Peek()).Returns(message);
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.TemporaryError);

            this.Forward.Send();

            this.Repository.Verify(r => r.Peek(), Times.Once());
            this.HttpClient.Verify(hc => hc.Post(message), Times.Once());
            this.Repository.Verify(r => r.Dequeue(), Times.Never());
        }

        [Fact]
        public void WhenCallingSendAndHasPermanentErrorShouldRemoveMessageFromQueue()
        {
            var messageCounter = 0;

            var message = new Message(null, null);

            this.Repository.Setup(r => r.Peek())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 2 ? message : (IMessage)null);
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.PermanentError);

            this.Forward.Send();

            this.Repository.Verify(r => r.Peek(), Times.Exactly(2));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Once());
            this.Repository.Verify(r => r.Dequeue(), Times.Once());
        }

        [Fact]
        public void WhenCallingSendAndMultipleMessagesInQueueAndSucceedsShouldLoopAndRemoveAllMessageFromQueue()
        {
            var messageCounter = 0;

            var message = new Message(null, null);

            this.Repository.Setup(r => r.Peek())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 5 ? message : (IMessage)null);
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.Ok);
            this.Repository.Setup(r => r.Dequeue()).Returns(message);

            this.Forward.Send();

            this.Repository.Verify(r => r.Peek(), Times.Exactly(5));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Exactly(4));
            this.Repository.Verify(r => r.Dequeue(), Times.Exactly(4));
        }
    }
}
