namespace reyna.Interfaces
{
    using System.Net;

    internal interface IHttpClient
    {
        Result Put(IMessage message);
    }
}
