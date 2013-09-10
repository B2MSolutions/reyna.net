namespace Reyna.Integration.Facts
{
    using System;
    using System.Net;
    using Reyna.Interfaces;
    using Xunit;

    public class GivenAHttpClient
    {
        [Fact]
        public void WhenCallingPostWithVaidUrlShouldSucceed()
        {
            var httpClient = new HttpClient();
            var message = new Message(new Uri("http://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);
            
            Assert.Equal(Result.Ok, result);
        }

        [Fact]
        public void WhenCallingPostWithInvalidUrlShouldReturnPermanentError()
        {
            var httpClient = new HttpClient();
            var message = new Message(new Uri("http://httpbin.org/post2"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.PermanentError, result);
        }

        [Fact]
        public void WhenCallingPostWithInvalidURLSchemeShouldReturnPermanentError()
        {
            var httpClient = new HttpClient();
            var message = new Message(new Uri("test://httpbin.org/post"), "{ \"lat\":51.527516, \"lng\":-0.715806, \"utc\":1362065860 }");
            message.Headers.Add("content-type", "application/json");
            message.Headers.Add("param1", "Value1");
            message.Headers.Add("param2", "Value2");
            message.Headers.Add("param3", "Value3");
            var result = httpClient.Post(message);

            Assert.Equal(Result.PermanentError, result);
        }

        [Fact]
        public void WhenCallingGetStatusCodeWithNullResponseShouldReturnServiceUnavailable()
        {
            var actual = HttpClient.GetStatusCode(null);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, actual);
        }
    }
}
