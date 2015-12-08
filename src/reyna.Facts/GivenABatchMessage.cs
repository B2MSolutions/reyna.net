namespace Reyna.Facts
{
    using System;
    using Xunit;

    public class GivenABatchMessage
    {
        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            var batchMessage = new BatchMessage(1, new Uri("http://post.com"), "payload");
            Assert.NotNull(batchMessage);
            Assert.Equal(1, batchMessage.ReynaId);
        }

        [Fact]
        public void WhenSettingUrlShouldThenReturnExpected()
        {
            var builder = new UriBuilder();
            builder.Host = "host";
            builder.Port = 8080;
            builder.Path = "path";

            var batchMessage = new BatchMessage(1, builder.Uri, "payload");

            Assert.Equal("http://host:8080/path", batchMessage.Url.ToString());
        }

        [Fact]
        public void WhenSettingPayloadShouldThenReturnExpected()
        {
            var batchMessage = new BatchMessage(12, new Uri("http://post.com"), "PAYLOAD");

            Assert.Equal("PAYLOAD", batchMessage.Payload);
            Assert.Equal(12, batchMessage.ReynaId);
        }

        [Fact]
        public void WhenCallingToJsonShouldReturnExpected()
        {
            var payload = "{\"Version Incremental\":\"20150514.093204\",\"User\":\"user\",\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}";
            var batchMessage = new BatchMessage(12, new Uri("http://post.com"), payload);

            var expected = "{\"url\":\"http://post.com/\", \"reynaId\":12, \"payload\":{\"Version Incremental\":\"20150514.093204\",\"User\":\"user\",\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}}";
            var actual = batchMessage.ToJson();
            Assert.Equal(expected, actual);
        }
    }
}
