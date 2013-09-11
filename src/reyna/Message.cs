namespace Reyna
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Reyna.Interfaces;

    public sealed class Message : IMessage
    {
        public Message(Uri url, string body)
        {
            this.Url = url;
            this.Body = body;
            this.Headers = new WebHeaderCollection();
        }

        public int Id { get; internal set; }

        public string Body { get; private set; }

        public Uri Url { get; private set; }

        public WebHeaderCollection Headers { get; private set; }
    }
}
