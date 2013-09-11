namespace Reyna.Facts
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAForwardService
    {
        public GivenAForwardService()
        {
            this.PersistentStore = new SQLiteRepository();
            
            this.HttpClient = new Mock<IHttpClient>();

            File.Delete(this.DatabasePath);
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Ok);

            this.PersistentStore.Initialise();

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object);
        }

        internal IRepository PersistentStore { get; set; }

        internal Mock<IHttpClient> HttpClient { get; set; }

        internal IService ForwardService { get; set; }

        private string DatabasePath
        {
            get
            {
                var assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(assemblyFile.DirectoryName, "reyna.db");
            }
        }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.ForwardService);
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedShouldCallPostOnHttpClient()
        {
            var message = this.CreateMessage();
            this.ForwardService.Start();

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            Assert.Null(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedThenImmediatelyStopShouldNotCallPostOnHttpClient()
        {
            var message = this.CreateMessage();

            this.ForwardService.Start();
            Thread.Sleep(50);

            this.PersistentStore.Add(message);
            this.ForwardService.Stop();
            Thread.Sleep(200);

            this.PersistentStore.Add(this.CreateMessage());
            Thread.Sleep(200);

            Assert.NotNull(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.AtMostOnce());
        }

        [Fact]
        public void WhenCallingStartThenStopThenStartShouldPostAllMessages()
        {
            var message = this.CreateMessage();

            this.ForwardService.Start();
            Thread.Sleep(50);

            this.PersistentStore.Add(message);
            this.ForwardService.Stop();
            Thread.Sleep(200);

            this.PersistentStore.Add(this.CreateMessage());
            this.PersistentStore.Add(this.CreateMessage());
            
            this.ForwardService.Start();
            Thread.Sleep(200);

            Assert.Null(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Exactly(3));
        }

        [Fact]
        public void WhenCallingStartAndStopRapidlyWhilstAddingMessagesShouldPostAllMessages()
        {
            var messageAddingThread = new Thread(new ThreadStart(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    this.PersistentStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
                    Thread.Sleep(100);
                }
            }));

            messageAddingThread.Start();
            Thread.Sleep(50);

            for (int k = 0; k < 10; k++)
            {
                this.ForwardService.Start();
                Thread.Sleep(50);

                this.ForwardService.Stop();
                Thread.Sleep(200);
            }

            Thread.Sleep(1000);

            Assert.Null(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Exactly(10));
        }

        [Fact]
        public void WhenCallingStopShouldExitImmediately()
        {
            for (int j = 0; j < 20; j++)
            {
                this.PersistentStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            }

            this.ForwardService.Start();
            Thread.Sleep(50);

            this.ForwardService.Stop();
            
            Thread.Sleep(1000);

            Assert.NotNull(this.PersistentStore.Get());
        }

        [Fact]
        public void WhenCallingStopOnForwareServiceThatHasntStartedShouldNotThrow()
        {
            this.ForwardService.Stop();
        }

        [Fact]
        public void WhenCallingDisposeShouldNotThrow()
        {
            this.ForwardService.Dispose();
        }

        [Fact]
        public void WhenConstructingWithBothNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, null));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullMessageStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, new Mock<IHttpClient>().Object));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullRepositoryParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, null));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public void WhenCallingStartStopDisposeShouldNotThrow()
        {
            this.ForwardService.Start();
            Thread.Sleep(50);

            this.ForwardService.Stop();
            Thread.Sleep(50);

            this.ForwardService.Dispose();
        }

        [Fact]
        public void WhenPostingMessagesAndPermanentErrorShouldRemoveMessageFromQueue()
        {
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.PermanentError);

            var message = this.CreateMessage();
            this.ForwardService.Start();

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            Assert.Null(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact]
        public void WhenPostingMessagesAndTemporaryErrorShouldNotRemoveMessageFromQueue()
        {
            this.HttpClient.Setup(hc => hc.Post(It.IsAny<IMessage>())).Returns(Result.TemporaryError);

            var message = this.CreateMessage();
            this.ForwardService.Start();

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            Assert.NotNull(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Once());
        }

        private IMessage CreateMessage()
        {
            return new Message(new Uri("http://test.com"), "BODY");
        }
    }
}
