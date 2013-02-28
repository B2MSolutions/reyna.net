namespace reyna
{
    using reyna.Interfaces;

    internal class Forward : IForward
    {
        public Forward(IRepository repository, IHttpClient httpClient)
        {
            this.Repository = repository;

            this.HttpClient = httpClient;
        }

        private IRepository Repository { get; set; }

        private IHttpClient HttpClient { get; set; }
        
        public void Send()
        {
            IMessage message = null;
            while((message = this.Repository.Peek()) != null)
            {
                if (this.HttpClient.Post(message) == Result.TemporaryError)
                {
                    return;
                }

                this.Repository.Dequeue();
            }

            //IMessage message = null;
            //do
            //{
            //    message = this.Repository.Peek();
            //    if (message == null)
            //    {
            //        return;
            //    }

            //    if (this.HttpClient.Put(message) == Result.TemporaryError)
            //    {
            //        return;
            //    }

            //    this.Repository.Dequeue();

            //} while (message != null);
        }
    }
}
