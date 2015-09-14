namespace Reyna.Interfaces
{
    using System.Net;

    internal interface IHttpClient
    {
        Result Post(IMessage message);
    }
}
