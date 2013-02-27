namespace reyna.Interfaces
{
    using System;
    using System.Net;

    public interface IMessage
    {
        string Body { get; set; }

        Uri Url { get; set; }

        WebHeaderCollection Headers { get; set; }
    }
}
