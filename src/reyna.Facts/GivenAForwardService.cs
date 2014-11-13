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
            this.NetworkStateService = new Mock<INetworkStateService>();
            this.WaitHandle = new AutoResetEventAdapter(false);

            File.Delete(this.DatabasePath);
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Ok);

            this.PersistentStore.Initialise();

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 100, 0);
        }

        private IRepository PersistentStore { get; set; }

        private Mock<IHttpClient> HttpClient { get; set; }

        private Mock<INetworkStateService> NetworkStateService { get; set; }

        private IWaitHandle WaitHandle { get; set; }

        private ForwardService ForwardService { get; set; }

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
        public void WhenConstructingWithAllNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, null, null, this.WaitHandle, 0, 0));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullSourceStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, new Mock<IHttpClient>().Object, new Mock<INetworkStateService>().Object, this.WaitHandle, 0, 0));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullHttpClientParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, null, new Mock<INetworkStateService>().Object, this.WaitHandle, 0, 0));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullNetworkStateParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, new Mock<IHttpClient>().Object, null, this.WaitHandle, 0, 0));
            Assert.Equal("networkState", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullWaitHandleStateParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, new Mock<IHttpClient>().Object, new Mock<INetworkStateService>().Object, null, 0, 0));
            Assert.Equal("waitHandle", exception.ParamName);
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

        [Fact]
        public void WhenCallingNetworkStateChangeShouldPostAllMessages()
        {
            var networkStateWaitHandle = new AutoResetEventAdapter(false);
            var networkState = new NetworkStateService(new Mock<ISystemNotifier>().Object, networkStateWaitHandle);

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, networkState, this.WaitHandle, 100, 0);

            var returnResult = Result.TemporaryError;
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(() => returnResult);

            var message = this.CreateMessage();

            this.ForwardService.Start();
            networkState.Start();
            Thread.Sleep(100);

            this.PersistentStore.Add(message);
            this.PersistentStore.Add(message);
            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            returnResult = Result.Ok;
            networkStateWaitHandle.Set();
            Thread.Sleep(5000);

            Assert.Null(this.PersistentStore.Get());
        }

        [Fact]
        public void WhenCallingNetworkStateChangeAndServiceStopedShouldNotPostAllMessages()
        {
            var networkStateWaitHandle = new AutoResetEventAdapter(false);
            var networkState = new NetworkStateService(new Mock<ISystemNotifier>().Object, networkStateWaitHandle);

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, networkState, this.WaitHandle, 100, 0);

            var returnResult = Result.TemporaryError;
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(() => returnResult);

            var message = this.CreateMessage();

            this.ForwardService.Start();
            networkState.Start();
            Thread.Sleep(50);

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            this.ForwardService.Stop();
            returnResult = Result.Ok;
            networkStateWaitHandle.Set();
            Thread.Sleep(500);

            Assert.NotNull(this.PersistentStore.Get());
        }

        [Fact]
        public void WhenReceivingTemporaryErrorMessageFromServerShouldSleepFor5Minutes()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.TemporaryError);

            var waitHandle = new Mock<IWaitHandle>();
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle.Object, 1000, 0);
            forwardService.Start();
            Thread.Sleep(500);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.Once());
        }

        [Fact]
        public void WhenSendingMessagesShouldSleepFor1SecondBetweenEachMessage()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Ok);

            var waitHandle = new Mock<IWaitHandle>();
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle.Object, 1000, 1000);
            forwardService.Start();
            Thread.Sleep(3000);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.AtMost(3));
        }

        [Fact]
        public void ShouldSetTemporaryErrorMillisecondsTo5Minutes()
        {
            var forwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 300000, 0);
            Assert.Equal(300000, forwardService.TemporaryErrorMilliseconds);
        }

        [Fact]
        public void ShouldSetSleepMillisecondsTo1Second()
        {
            var forwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 300000, 1000);
            Assert.Equal(1000, forwardService.SleepMilliseconds);
        }

        private IMessage CreateMessage()
        {
            return new Message(new Uri("http://test.com"), "BODY");
        }
    }
}
