namespace reyna
{
    using System;
    using System.Net;
    using reyna.Interfaces;

    public class Message : IMessage
    {
        public Message()
        {
            this.Headers = new WebHeaderCollection();
        }

        public string Body { get; set; }

        public Uri Url { get; set; }

        public WebHeaderCollection Headers { get; set; }
    }
}
