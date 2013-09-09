namespace Reyna.Integration.Facts
{
    using System;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAHttpClient
    {
        [Fact]
        public void WhenCallingPost()
        {
            var httpClient = new HttpClient();
            var message = new Message(new Uri("http://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("token", "6dec1821543f4a82a845f81109a47aee");
            message.Headers.Add("id", "0123456789");
            message.Headers.Add("scheme", "IMEI");
            var result = httpClient.Post(message);
            
            Assert.Equal(Result.Ok, result);
        }

        [Fact]
        public void WhenCallingPost3()
        {
            var httpClient = new HttpClient();
            var message = new Message(new Uri("http://httpbin.org/post2"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("token", "6dec1821543f4a82a845f81109a47aee");
            message.Headers.Add("id", "0123456789");
            message.Headers.Add("scheme", "IMEI");
            var result = httpClient.Post(message);

            Assert.Equal(Result.PermanentError, result);
        }
    }
}
