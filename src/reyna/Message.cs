namespace reyna
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using reyna.Interfaces;

    public class Message : IMessage
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

        public object Clone()
        {
            var clone = new Message(this.Url, this.Body)
            {
                Id = this.Id
            };
            foreach(string key in this.Headers)
            {
                clone.Headers.Add(key, this.Headers[key]);
            }

            return clone;
        }
    }
}
