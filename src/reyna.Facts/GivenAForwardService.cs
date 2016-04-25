namespace Reyna.Facts
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using Moq;
    using OpenNETCF.Net.NetworkInformation;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAForwardService
    {
        public GivenAForwardService()
        {
            NetworkInterface.NetworkInterfaces = new INetworkInterface[0];
            this.Preferences = new Preferences();
            this.Preferences.ResetCellularDataBlackout();
            this.Preferences.ResetWlanBlackoutRange();
            this.Preferences.ResetWwanBlackoutRange();
            this.Preferences.ResetRoamingBlackout();
            this.Preferences.ResetOnChargeBlackout();
            this.Preferences.ResetOffChargeBlackout();

            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "wifi";

            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            this.PersistentStore = new SQLiteRepository();
            this.HttpClient = new Mock<IHttpClient>();
            this.NetworkStateService = new Mock<INetworkStateService>();
            this.PeriodicBackoutCheck = new Mock<IPeriodicBackoutCheck>();
            this.Logger = new Mock<IReynaLogger>();

            this.WaitHandle = new AutoResetEventAdapter(false);

            File.Delete(this.DatabasePath);
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Ok);

            this.PersistentStore.Initialise();

            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 100))
                .Returns(true);

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 100, 0, false, this.Logger.Object);
            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna\PeriodicBackoutCheck", false);
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKey(@"Software\Reyna", false);
        }

        public Preferences Preferences { get; set; }

        private IRepository PersistentStore { get; set; }

        private Mock<IHttpClient> HttpClient { get; set; }

        private Mock<IPeriodicBackoutCheck> PeriodicBackoutCheck { get; set; }

        private Mock<INetworkStateService> NetworkStateService { get; set; }

        private Mock<IReynaLogger> Logger { get; set; }

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
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 100, 0, false, this.Logger.Object);
            
            Assert.NotNull(this.ForwardService);
            Assert.NotNull(this.ForwardService.MessageProvider);
            Assert.NotNull(this.ForwardService.PeriodicBackoutCheck);
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
        public void WhenCallingStartAndItIsBlackoutShouldNotCallPostOnHttpClientOrDeleteMessage()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = new IPAddress(42);
            networkInterface.Name = "cellular line";
            var from = new Time();
            var to = new Time(from.MinuteOfDay + 1);
            this.Preferences.SetCellularDataBlackout(new TimeRange(from, to));
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var message = this.CreateMessage();
            this.ForwardService.Start();

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            Assert.NotNull(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Never());
        }

        [Fact]
        public void WhenCallingStartAndNotConnectedShouldNotCallPostOnHttpClientOrDeleteMessage()
        {
            var networkInterface = new NetworkInterface();
            networkInterface.CurrentIpAddress = null;
            NetworkInterface.NetworkInterfaces = new INetworkInterface[] { networkInterface };

            var message = this.CreateMessage();
            this.ForwardService.Start();

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            Assert.NotNull(this.PersistentStore.Get());
            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Never());
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
            for (int j = 0; j < 50; j++)
            {
                this.PersistentStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            }

            this.ForwardService.Start();
            Thread.Sleep(50);

            this.ForwardService.Stop();
            
            Thread.Sleep(200);

            Assert.NotNull(this.PersistentStore.Get());
        }

        [Fact]
        public void WhenCallingStopOnForwareServiceThatHasNotStartedShouldNotThrow()
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
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, null, null, this.WaitHandle, 0, 0, false, this.Logger.Object));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullSourceStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(null, new Mock<IHttpClient>().Object, new Mock<INetworkStateService>().Object, this.WaitHandle, 0, 0, false, this.Logger.Object));
            Assert.Equal("sourceStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullHttpClientParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, null, new Mock<INetworkStateService>().Object, this.WaitHandle, 0, 0, false, this.Logger.Object));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullNetworkStateParameterShouldNotThrow()
        {
            var forwardService = new ForwardService(new Mock<IRepository>().Object, new Mock<IHttpClient>().Object, null, this.WaitHandle, 0, 0, false, this.Logger.Object);
            Assert.NotNull(forwardService);
        }

        [Fact]
        public void WhenConstructingWithNullWaitHandleStateParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ForwardService(new Mock<IRepository>().Object, new Mock<IHttpClient>().Object, new Mock<INetworkStateService>().Object, null, 0, 0, false, this.Logger.Object));
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
            this.PeriodicBackoutCheck.Verify(p => p.Record("ForwardService"), Times.Never());
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
            this.PeriodicBackoutCheck.Verify(p => p.Record("ForwardService"), Times.Once());
        }

        [Fact]
        public void WhenCallingNetworkStateChangeShouldPostAllMessages()
        {
            var networkStateWaitHandle = new AutoResetEventAdapter(false);
            var networkState = new NetworkStateService(new Mock<ISystemNotifier>().Object, networkStateWaitHandle);

            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 100))
               .Returns(true);

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, networkState, this.WaitHandle, 100, 0, false, this.Logger.Object);
            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

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
            Thread.Sleep(6000);

            Assert.Null(this.PersistentStore.Get());

            this.PeriodicBackoutCheck.Verify(p => p.Record("ForwardService"), Times.AtLeast(1));
        }

        [Fact]
        public void WhenCallingNetworkStateChangeAndServiceStopedShouldNotPostAllMessages()
        {
            var networkStateWaitHandle = new AutoResetEventAdapter(false);
            var networkState = new NetworkStateService(new Mock<ISystemNotifier>().Object, networkStateWaitHandle);

            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, networkState, this.WaitHandle, 100, 0, false, this.Logger.Object);

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

            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 1000))
                .Returns(true);

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();
            Thread.Sleep(500);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.Once());
            this.PeriodicBackoutCheck.Verify(p => p.Record("ForwardService"), Times.Once());
        }

        [Fact]
        public void WhenSendingMessagesShouldSleepFor1SecondBetweenEachMessage()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Ok);

            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 100, 1000, false, this.Logger.Object);
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();
            Thread.Sleep(3000);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.AtLeast(3));
        }

        [Fact]
        public void ShouldSetTemporaryErrorMillisecondsTo5Minutes()
        {
            var forwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 300000, 0, false, this.Logger.Object);
            Assert.Equal(300000, forwardService.TemporaryErrorMilliseconds);
        }

        [Fact]
        public void ShouldSetSleepMillisecondsTo1Second()
        {
            var forwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 300000, 1000, false, this.Logger.Object);
            Assert.Equal(1000, forwardService.SleepMilliseconds);
        }

        [Fact]
        public void WhenReceivingBlackoutErrorShouldNotSleepAndNotDeletingMessages()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.Blackout);

            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 1000))
               .Returns(true);
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();
            Thread.Sleep(500);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.Once());
            store.Verify(s => s.Remove(), Times.Never());
        }

        [Fact]
        public void WhenReceivingNotConnectedErrorShouldNotSleepAndNotDeletingMessages()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.NotConnected);

            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 1000))
               .Returns(true);
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();
            Thread.Sleep(500);
            forwardService.Stop();

            store.Verify(s => s.Get(), Times.Once());
            store.Verify(s => s.Remove(), Times.Never());
        }

        [Fact]
        public void WhenReceivingTemporaryErrorMessageFromServerAndSleepFor5MinutesThenTerminateSignaledShouldExit()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(Result.TemporaryError);

            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 5 * 60 * 1000, 0, false, this.Logger.Object);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 5 * 60 * 1000))
                .Returns(true);
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            new Thread(this.StopForwardService).Start(forwardService);
            forwardService.Start();
            Thread.Sleep(1000);

            store.Verify(s => s.Get(), Times.Once());
        }

        [Fact]
        public void WhenCallingResumeShouldPostAllMessages()
        {
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, null, this.WaitHandle, 100, 0, false, this.Logger.Object);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 100))
              .Returns(true);

            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            var returnResult = Result.TemporaryError;
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(() => returnResult);

            var message = this.CreateMessage();

            this.ForwardService.Start();
            Thread.Sleep(100);

            this.PersistentStore.Add(message);
            this.PersistentStore.Add(message);
            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            returnResult = Result.Ok;
            this.ForwardService.Resume();
            Thread.Sleep(6000);
            Assert.Null(this.PersistentStore.Get());
        }

        [Fact]
        public void WhenCallingResumeAndServiceStopedShouldNotPostAllMessages()
        {
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, null, this.WaitHandle, 100, 0, false, this.Logger.Object);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 100))
             .Returns(true);

            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            var returnResult = Result.TemporaryError;
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
                .Returns(() => returnResult);

            var message = this.CreateMessage();

            this.ForwardService.Start();
            Thread.Sleep(50);

            this.PersistentStore.Add(message);
            Thread.Sleep(200);

            this.ForwardService.Stop();
            returnResult = Result.Ok;
            this.ForwardService.Resume();
            Thread.Sleep(500);

            Assert.NotNull(this.PersistentStore.Get());
            this.PeriodicBackoutCheck.Verify(p => p.Record("ForwardService"), Times.Once());
            this.PeriodicBackoutCheck.Verify(p => p.IsTimeElapsed("ForwardService", 100), Times.AtLeast(1));
        }

        [Fact]
        public void WhenConstructingWithBatchModeEnabledShouldUseBatchProvider()
        {
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 100, 0, true, this.Logger.Object);
            Assert.NotNull(this.ForwardService);
            Assert.NotNull(this.ForwardService.MessageProvider);
            Assert.Equal(typeof(BatchProvider), this.ForwardService.MessageProvider.GetType());
        }

        [Fact]
        public void WhenConstructingWithBatchModeDisabledShouldUseMessageProvider()
        {
            this.ForwardService = new ForwardService(this.PersistentStore, this.HttpClient.Object, this.NetworkStateService.Object, this.WaitHandle, 100, 0, false, this.Logger.Object);
            Assert.NotNull(this.ForwardService);
            Assert.NotNull(this.ForwardService.MessageProvider);
            Assert.Equal(typeof(MessageProvider), this.ForwardService.MessageProvider.GetType());
        }

        [Fact]
        public void WhenMessageProviderCannotSendShouldNotSleepAndNotDeletingMessages()
        {
            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            store.Setup(s => s.Get()).Returns(this.CreateMessage());

            var messageProvider = new Mock<IMessageProvider>();
            messageProvider.SetupGet(m => m.CanSend).Returns(false);
            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            forwardService.MessageProvider = messageProvider.Object;
            forwardService.Start();

            Thread.Sleep(500);
            forwardService.Stop();

            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Never());
            messageProvider.Verify(m => m.CanSend, Times.AtLeast(1));
            messageProvider.Verify(m => m.GetNext(), Times.Never());
            messageProvider.Verify(m => m.Delete(It.IsAny<IMessage>()), Times.Never());
            messageProvider.Verify(m => m.Close(), Times.Never());
        }

        [Fact]
        public void WhenMessageProviderCanSendShouldCallClose()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
               .Returns(Result.Ok);

            var store = new Mock<IRepository>();
            var waitHandle = new AutoResetEventAdapter(false);
            var messageProvider = new Mock<IMessageProvider>();
            messageProvider.SetupGet(m => m.CanSend).Returns(true);
            messageProvider.Setup(s => s.GetNext()).Returns(this.CreateMessage());
            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            forwardService.MessageProvider = messageProvider.Object;
            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();

            Thread.Sleep(500);
            forwardService.Stop();

            messageProvider.Verify(m => m.CanSend, Times.AtLeast(1));
            messageProvider.Verify(m => m.GetNext(), Times.AtLeast(1));
            messageProvider.Verify(m => m.Delete(It.IsAny<IMessage>()), Times.AtLeast(1));
            messageProvider.Verify(m => m.Close(), Times.AtLeast(1));
        }

        [Fact]
        public void WhenThrowsWhileForwardingMessagesShouldNotStopThreadAndContinueSending()
        {
            this.HttpClient.Setup(c => c.Post(It.IsAny<IMessage>()))
               .Returns(Result.Ok);

            var store = new Mock<IRepository>();

            var waitHandle = new AutoResetEventAdapter(false);
            var messageProvider = new Mock<IMessageProvider>();
            messageProvider.SetupGet(m => m.CanSend).Returns(true);
            messageProvider.Setup(s => s.GetNext()).Returns(this.CreateMessage());

            var exception = new InvalidOperationException("Error");
            var callCount = 0;
            messageProvider.Setup(s => s.Delete(It.IsAny<IMessage>()))
                 .Callback(() => 
                     {
                         callCount++;
                         if (callCount % 2 == 0)
                         {
                             throw exception;
                         }
                     });

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            forwardService.MessageProvider = messageProvider.Object;
            this.ForwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;

            forwardService.Start();

            Thread.Sleep(3000);
            forwardService.Stop();

            messageProvider.Verify(m => m.CanSend, Times.AtLeast(1));
            messageProvider.Verify(m => m.GetNext(), Times.AtLeast(1));
            messageProvider.Verify(m => m.Delete(It.IsAny<IMessage>()), Times.AtLeast(1));
            messageProvider.Verify(m => m.Close(), Times.AtLeast(1));
            this.Logger.Verify(l => l.Err("ForwardService.ThreadStart. Error {0}", exception.ToString()), Times.AtLeast(1));
        }

        [Fact]
        public void WhenGettingTemporaryErrorShoudBackoutForFiveMinutes()
        {
            var waitHandle = new AutoResetEventAdapter(false);
            var store = new Mock<IRepository>();
            var messageProvider = new Mock<IMessageProvider>();
            messageProvider.SetupGet(m => m.CanSend).Returns(true);
            this.PeriodicBackoutCheck.Setup(p => p.IsTimeElapsed("ForwardService", 1000))
                .Returns(false);

            var forwardService = new ForwardService(store.Object, this.HttpClient.Object, this.NetworkStateService.Object, waitHandle, 1000, 0, false, this.Logger.Object);
            forwardService.MessageProvider = messageProvider.Object;
            forwardService.PeriodicBackoutCheck = this.PeriodicBackoutCheck.Object;
            forwardService.Start();

            Thread.Sleep(500);
            forwardService.Stop();

            this.HttpClient.Verify(c => c.Post(It.IsAny<IMessage>()), Times.Never());
            messageProvider.Verify(m => m.CanSend, Times.AtLeast(1));
            messageProvider.Verify(m => m.GetNext(), Times.Never());
            messageProvider.Verify(m => m.Delete(It.IsAny<IMessage>()), Times.Never());
            messageProvider.Verify(m => m.Close(), Times.Never());
            this.PeriodicBackoutCheck.Verify(p => p.IsTimeElapsed("ForwardService", 1000), Times.AtLeast(1));
        }

        private void StopForwardService(object forwardService)
        {
            Thread.Sleep(100);
            ((ForwardService)forwardService).Stop();
        }

        private IMessage CreateMessage()
        {
            return new Message(new Uri("http://test.com"), "BODY");
        }
    }
}
