namespace Reyna
{
    using Reyna.Interfaces;

    internal class BatchProvider : IMessageProvider
    {
        public BatchProvider(IRepository repository)
        {
            this.Repository = repository;
        }

        public bool CanSend
        {
            get
            {
                return false;
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
    }
}
