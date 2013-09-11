namespace Reyna
{
    using Reyna.Interfaces;

    internal sealed class ForwardService : IService
    {
        public ForwardService(IRepository persistentStore, IHttpClient httpClient)
        {
            this.PersistentStore = persistentStore;

            this.HttpClient = httpClient;
        }

        private IRepository PersistentStore { get; set; }

        private IHttpClient HttpClient { get; set; }
        
        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
