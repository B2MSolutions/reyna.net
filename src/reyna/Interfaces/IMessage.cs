namespace Reyna.Interfaces
{
    using System;
    using System.Net;

    public interface IMessage
    {
        int Id { get; }

        string Body { get; }

        Uri Url { get; }

        WebHeaderCollection Headers { get; }
    }
}
