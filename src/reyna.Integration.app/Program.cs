namespace Reyna.Integration.app
{
    using System;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var store = new ReynaService();

            store.Start();

            store.Put(GetMessage());
            store.Put(GetMessage());
            store.Put(GetMessage());

            Thread.Sleep(200);

            store.Put(GetMessage());
            store.Put(GetMessage());
            store.Put(GetMessage());

            store.Stop();
        }

        private static Message GetMessage()
        {
            var message = new Message(new Uri("http://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");

            return message;
        }
    }
}
