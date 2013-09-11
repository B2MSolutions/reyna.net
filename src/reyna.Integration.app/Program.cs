namespace Reyna.Integration.app
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            var store = new ReynaService();
            var message = new Message(new Uri("https://api.test.com/api/1"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            store.Put(message);
        }
    }
}
