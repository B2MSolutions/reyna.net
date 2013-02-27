namespace reyna.Interfaces
{
    using System;
    using System.Net;

    public interface IMessage : ICloneable
    {
        int Id { get; }

        string Body { get; }

        Uri Url { get; }

        WebHeaderCollection Headers { get; }
    }
}
