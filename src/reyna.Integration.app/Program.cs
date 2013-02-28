namespace reyna.Integration.app
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var store = new Store();
            var message = new Message(new Uri("https://api.mprodigy.com/api/1/geo"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("token", "6dec1821543f4a82a845f81109a47aee");
            message.Headers.Add("id", "0123456789");
            message.Headers.Add("scheme", "IMEI");
            store.Put(message);
        }
    }
}
