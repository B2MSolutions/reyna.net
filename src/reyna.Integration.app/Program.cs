namespace reyna.Integration.app
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var store = new Store();
            var message = new Message(new Uri("http://test.com:8090"), "body");
            message.Headers.Add("Content_type", "application/json");
            store.Put(message);
        }
    }
}
