namespace reyna
{
    using reyna.Interfaces;

    public class Store : IStore
    {
        private const string DatabaseName = "reyna.db";

        public Store()
        {
            this.Repository = new SQLiteRepository();
        }

        internal IRepository Repository { get; set; }

        public void Put(IMessage message)
        {
            if (!this.Repository.DoesExist(Store.DatabaseName))
            {
                this.Repository.Create(Store.DatabaseName);
            }

            this.Repository.Insert(message);
        }
    }
}
