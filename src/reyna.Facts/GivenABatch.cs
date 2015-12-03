namespace Reyna.Facts
{
    using System;
    using Xunit;

    public class GivenABatch
    {
        [Fact]
        public void WhenConstructingShouldNotThrow()
        {
            var batch = new Batch();
            Assert.NotNull(batch);
            Assert.NotNull(batch.Events);
        }

        [Fact]
        public void WhenCallingAddShouldAddNewMessage()
        {
            var batchMessage = new Message(new Uri("http://post.com/1/info"), "payload");
            batchMessage.Id = 1;
            var batch = new Batch();
            batch.Add(batchMessage);

            Assert.Equal(1, batch.Events.Count);
            var message = batch.Events[0];
            Assert.Equal("http://post.com/1/info", message.Url.AbsoluteUri);
            Assert.Equal(1, message.ReynaId);
            Assert.Equal("payload", message.Payload);
        }

        [Fact]
        public void WhenRemoveLastMessageAndOnlyOneMessageAvailableShouldNotRemoveIt()
        {
            var batchMessage = new Message(new Uri("http://post.com/api"), "payload");
            batchMessage.Id = 1;
            var batch = new Batch();
            batch.Add(batchMessage);

            batch.RemoveLastMessage();

            Assert.Equal(1, batch.Events.Count);
            var message = batch.Events[0];
            Assert.Equal("http://post.com/api", message.Url.ToString());
            Assert.Equal(1, message.ReynaId);
            Assert.Equal("payload", message.Payload);
        }

        [Fact]
        public void WhenRemoveLastMessageShouldRemoveIt()
        {
            var batchMessage = new Message(new Uri("http://post.com"), "payload");
            batchMessage.Id = 1;
            var batch = new Batch();
            batch.Add(batchMessage);
            batchMessage = new Message(new Uri("http://post2.com"), "payload2");
            batchMessage.Id = 2;
            batch.Add(batchMessage);
            batchMessage = new Message(new Uri("http://post3.com"), "payload3");
            batchMessage.Id = 3;
            batch.Add(batchMessage);

            batch.RemoveLastMessage();

            Assert.Equal(2, batch.Events.Count);
            var message = batch.Events[0];
            Assert.Equal("http://post.com/", message.Url.AbsoluteUri);
            Assert.Equal(1, message.ReynaId);
            Assert.Equal("payload", message.Payload);
            
            message = batch.Events[1];
            Assert.Equal("http://post2.com/", message.Url.ToString());
            Assert.Equal(2, message.ReynaId);
            Assert.Equal("payload2", message.Payload);
        }
        
        [Fact]
        public void WhenCallingToJsonShouldReturnExpected()
        {
            var payload1 = "{\"Version Incremental\":\"20150514.093204\",\"User\":\"user\",\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}";
            var batchMessage1 = new Message(new Uri("http://post.com"), payload1);
            batchMessage1.Id = 12;

            var payload2 = "{\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}";
            var batchMessage2 = new Message(new Uri("http://post2.com"), payload2);
            batchMessage2.Id = 14;

            var batch = new Batch();
            batch.Add(batchMessage1);
            batch.Add(batchMessage2);

            var expected = "{\"events\":[{\"url\":\"http://post.com/\", \"reynaId\":12, \"payload\":{\"Version Incremental\":\"20150514.093204\",\"User\":\"user\",\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}}, {\"url\":\"http://post2.com/\", \"reynaId\":14, \"payload\":{\"Brand\":\"os\",\"device\":\"device\",\"Network Operator Name\":\"unknown\",\"Manufacturer\":\"MM\",\"updated\":1448534970738,\"list\":[{\"name\":\"System\",\"version\":\"4.1.1\",\"flag\":true},{\"name\":\"backupcon\",\"version\":\"4.1.1\",\"flag\":false}],\"Version Release\":\"4.1.1\",\"MAC\":\"MAC\"}}]}";
            var actual = batch.ToJson();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WhenCallingToJsonWithEmptyEventsShouldReturnExpected()
        {
            var batch = new Batch();
            
            var expected = "{\"events\":[]}";
            var actual = batch.ToJson();
            Assert.Equal(expected, actual);
        }
    }
}
