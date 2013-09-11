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

            this.Repository.Setup(r => r.Get())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 2 ? message : (IMessage)null);

            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.Ok);
            this.Repository.Setup(r => r.Remove()).Returns(message);

            this.Forward.Send();

            this.Repository.Verify(r => r.Get(), Times.Exactly(2));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Once());
            this.Repository.Verify(r => r.Remove(), Times.Once());
        }

        [Fact]
        public void WhenCallingSendAndNoMessageShouldDoNothing()
        {
            this.Repository.Setup(r => r.Get()).Returns((IMessage)null);

            this.Forward.Send();

            this.Repository.Verify(r => r.Get(), Times.Once());
            this.HttpClient.Verify(hc => hc.Post(It.IsAny<IMessage>()), Times.Never());
            this.Repository.Verify(r => r.Remove(), Times.Never());
        }

        [Fact]
        public void WhenCallingSendAndHasPermanentErrorShouldRemoveMessageFromQueue()
        {
            var messageCounter = 0;

            var message = new Message(null, null);

            this.Repository.Setup(r => r.Get())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 2 ? message : (IMessage)null);
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.PermanentError);

            this.Forward.Send();

            this.Repository.Verify(r => r.Get(), Times.Exactly(2));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Once());
            this.Repository.Verify(r => r.Remove(), Times.Once());
        }

        [Fact]
        public void WhenCallingSendAndMultipleMessagesInQueueAndSucceedsShouldLoopAndRemoveAllMessageFromQueue()
        {
            var messageCounter = 0;

            var message = new Message(null, null);

            this.Repository.Setup(r => r.Get())
                .Callback(() => messageCounter++)
                .Returns(() => messageCounter < 5 ? message : (IMessage)null);
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.Ok);
            this.Repository.Setup(r => r.Remove()).Returns(message);

            this.Forward.Send();

            this.Repository.Verify(r => r.Get(), Times.Exactly(5));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Exactly(4));
            this.Repository.Verify(r => r.Remove(), Times.Exactly(4));
        }

        [Fact]
        public void WhenCallingSendWithMultipleMessagesInQueueAndFirstFailsWithTemporaryErrorShouldStillSendSecond()
        {
            var message = new Message(null, null);

            var peekCount = 0;
            var postCount = 0;

            this.Repository.Setup(r => r.Get())
                .Callback(() => peekCount++)
                .Returns(() => peekCount < 3 ? message : null);

            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>()))
                .Callback(() => postCount++)
                .Returns(() => postCount == 1 ? Result.TemporaryError : Result.Ok);

            this.Forward.Send();

            this.Repository.Verify(r => r.Get(), Times.Exactly(3));
            this.HttpClient.Verify(hc => hc.Post(message), Times.Exactly(2));
            this.Repository.Verify(r => r.Remove(), Times.Once());
        }
    }
}
