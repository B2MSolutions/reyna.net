namespace reyna
{
    using reyna.Interfaces;

    public class Store : IStore
    {
        public Store()
        {
            this.Repository = new SQLiteRepository();
            this.Forward = new Forward(this.Repository, new HttpClient());
        }

        internal IRepository Repository { get; set; }

        internal IForward Forward { get; set; }

        public void Put(IMessage message)
        {
            if (this.Repository.DoesNotExist)
            {
                this.Repository.Create();
            }

            this.Repository.Enqueue(message);
            
            // TODO
            // need to run in a separate thread
            this.Forward.Send();
        }
    }
}
