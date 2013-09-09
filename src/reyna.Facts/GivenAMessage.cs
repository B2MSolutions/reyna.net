namespace Reyna.Facts
{
    using System;
    using Xunit;

    public class GivenAMessage
    {
        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            var message = new Message(null, null);
            Assert.NotNull(message);
        }

        [Fact]
        public void WhenSettingUrlShouldThenReturnExpected()
        {
            var builder = new UriBuilder();
            builder.Host = "host";
            builder.Port = 8080;
            builder.Path = "path";

            var message = new Message(builder.Uri, null);

            Assert.Equal("http://host:8080/path", message.Url.ToString());
        }

        [Fact]
        public void WhenSettingBodyShouldThenReturnExpected()
        {
            var message = new Message(null, "BODY");

            Assert.Equal("BODY", message.Body);
        }

        [Fact]
        public void WhenConstructingShouldHaveEmptyHeaders()
        {
            var message = new Message(null, null);
            Assert.NotNull(message.Headers);
            Assert.Equal(0, message.Headers.Count);
        }

        [Fact]
        public void WhenAddingHeaderShouldHaveSingleHeader()
        {
            var message = new Message(null, null);
            message.Headers.Add("ContentType", "application/json");

            Assert.Equal(1, message.Headers.Count);
            Assert.Equal("application/json", message.Headers["ContentType"]);
        }
    }
}
