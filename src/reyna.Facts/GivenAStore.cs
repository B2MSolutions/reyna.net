namespace Reyna.Facts
{
    using System;
    using System.Threading;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAStore
    {
        [Fact]
        public void WhenCallingStartAndMessageAddedShouldCallPutOnRepository()
        {
            var messageStore = new InMemoryQueue();
            var repository = new Mock<IRepository>();
            repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var store = new Store(messageStore, repository.Object);

            store.Start();
            messageStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            Thread.Sleep(200);

            Assert.Null(messageStore.Get());
            repository.Verify(r => r.Enqueue(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndMessageAddedThenImmediatelyStopShouldNotCallPutOnRepository()
        {
            var messageStore = new InMemoryQueue();
            var repository = new Mock<IRepository>();
            repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var store = new Store(messageStore, repository.Object);

            store.Start();
            Thread.Sleep(50);
            messageStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            store.Stop();
            Thread.Sleep(200);
            messageStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
            Thread.Sleep(200);

            Assert.NotNull(messageStore.Get());
            repository.Verify(r => r.Enqueue(It.IsAny<IMessage>()), Times.Once());
        }

        [Fact]
        public void WhenCallingStartAndStopRapidlyWhilstAddingMessagesShouldNotCallPutOnRepository()
        {
            var messageStore = new InMemoryQueue();
            var repository = new Mock<IRepository>();
            repository.Setup(r => r.Enqueue(It.IsAny<IMessage>()));

            var store = new Store(messageStore, repository.Object);

            var messageAddingThread = new Thread(new ThreadStart(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        messageStore.Add(new Message(new Uri("http://www.google.com"), string.Empty));
                        Thread.Sleep(100);
                    }
                }));

            messageAddingThread.Start();
            Thread.Sleep(50);

            for (int k = 0; k < 10; k++)
            {
                store.Start();
                Thread.Sleep(50);
                store.Stop();
                Thread.Sleep(200);
            }

            Thread.Sleep(1000);

            Assert.Null(messageStore.Get());
            repository.Verify(r => r.Enqueue(It.IsAny<IMessage>()), Times.Exactly(10));
        }

        [Fact]
        public void WhenCallingStopOnStoreThatHasntStartedShouldNotThrow()
        {
            var messageStore = new InMemoryQueue();
            var repository = new Mock<IRepository>();
            var store = new Store(messageStore, repository.Object);
            
            store.Stop();
        }

        [Fact]
        public void WhenCallingDisposeShouldNotThrow()
        {
            var messageStore = new InMemoryQueue();
            var repository = new Mock<IRepository>();

            var store = new Store(messageStore, repository.Object);

            store.Dispose();
        }

        [Fact]
        public void WhenConstructingWithBothNullParametersShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new Store(null, null));
            Assert.Equal("messageStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullMessageStoreParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new Store(null, new Mock<IRepository>().Object));
            Assert.Equal("messageStore", exception.ParamName);
        }

        [Fact]
        public void WhenConstructingWithNullRepositoryParameterShouldThrow()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new Store(new InMemoryQueue(), null));
            Assert.Equal("repository", exception.ParamName);
        }
    }
}
