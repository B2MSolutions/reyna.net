namespace Reyna.Facts
{
    using System;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAForwardService
    {
        public GivenAForwardService()
        {
            this.PersistentStore = new Mock<IRepository>();
            
            this.HttpClient = new Mock<IHttpClient>();

            this.ForwardService = new ForwardService(this.PersistentStore.Object, this.HttpClient.Object);
        }

        internal Mock<IRepository> PersistentStore { get; set; }

        internal Mock<IHttpClient> HttpClient { get; set; }

        internal IService ForwardService { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ForwardService);
        }

        [Fact]
        public void WhenCallingStartShouldThrow()
        {
            Assert.Throws<NotImplementedException>(() => this.ForwardService.Start());
        }

        [Fact]
        public void WhenCallingStopShouldThrow()
        {
            Assert.Throws<NotImplementedException>(() => this.ForwardService.Stop());
        }

        [Fact]
        public void WhenCallingDisposehouldNotThrow()
        {
            this.ForwardService.Dispose();
        }

        /*        [Fact]
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
                }*/
    }
}
