namespace Reyna
{
    using Reyna.Interfaces;

    internal class MessageProvider : IMessageProvider
    {
        public MessageProvider(IRepository repository)
        {
            this.Repository = repository;
        }

        public bool CanSend
        {
            get
            {
                return true;
            }
        }

        private IRepository Repository { get; set; }

        public IMessage GetNext()
        {
            return this.Repository.Get();
        }

        public void Delete(IMessage message)
        {
            this.Repository.Remove();
        }

        public void Close()
        {
        }
    }
}
