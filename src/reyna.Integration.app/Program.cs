namespace Reyna.Integration.app
{
    using System;
    using System.Threading;

    public class Program
    {
        public static void Main(string[] args)
        {
            var store = new ReynaService(new NullLogger());

            store.Start();
            Time from = new Time(10, 00);
            Time to = new Time(12, 59);
            ReynaService.SetCellularDataBlackout(new TimeRange(from, to));

            store.Put(GetMessage());
            store.Put(GetMessage());
            store.Put(GetMessage());

            Thread.Sleep(100);

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
