namespace Reyna.Facts
{
    using System;
    using Moq;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAMessageProvider
    {
        public GivenAMessageProvider()
        {
            this.Repository = new Mock<IRepository>();
            this.MessageProvider = new MessageProvider(this.Repository.Object);
        }

        private Mock<IRepository> Repository { get; set; }

        private MessageProvider MessageProvider { get; set; }

        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            Assert.NotNull(this.MessageProvider);
        }

        [Fact]
        public void WhenCallingCanSendShouldRerurnTrue()
        {
            Assert.True(this.MessageProvider.CanSend);
        }

        [Fact]
        public void WhenCallingDeleteShouldRemoveMessage()
        {
            this.MessageProvider.Delete(It.IsAny<IMessage>());
            
            this.Repository.Verify(r => r.Remove());
        }

        [Fact]
        public void WhenCallingGetNextShouldGetNextAvailableMessage()
        {
            var message = new Mock<IMessage>();
            this.Repository.Setup(r => r.Get()).Returns(message.Object);

            var actual = this.MessageProvider.GetNext();

            this.Repository.Verify(r => r.Get());
            Assert.Same(message.Object, actual);
        }

        [Fact]
        public void WhenCallingCloseShouldDoNothing()
        {
            this.MessageProvider.Close();
        }
    }
}
