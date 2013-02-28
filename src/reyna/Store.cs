namespace reyna
{
    using reyna.Interfaces;

    public class Store : IStore
    {
        public Store()
        {
            this.Repository = new SQLiteRepository();
        }

        internal IRepository Repository { get; set; }

        public void Put(IMessage message)
        {
            if (this.Repository.DoesNotExist)
            {
                this.Repository.Create();
            }

            this.Repository.Enqueue(message);
        }
    }
}
